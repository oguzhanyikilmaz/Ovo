using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO;
using OVO.AiPipeline;
using OVO.Permissions;
using OVO.Users;
using OVO.Wardrobe;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace OVO.TryOn;

[Authorize(OVOPermissions.TryOn.Default)]
public class TryOnAppService : OVOAppService, ITryOnAppService
{
    private const int MinGarmentCount = 3;
    private const int FreeDailyRenderLimit = 20;

    private readonly IUserPhotoRepository _userPhotoRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IGarmentRepository _garmentRepository;
    private readonly IRenderCacheRepository _renderCacheRepository;
    private readonly IAiPipelineService _aiPipeline;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public TryOnAppService(
        IUserPhotoRepository userPhotoRepository,
        IUserProfileRepository userProfileRepository,
        IGarmentRepository garmentRepository,
        IRenderCacheRepository renderCacheRepository,
        IAiPipelineService aiPipeline,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _userPhotoRepository = userPhotoRepository;
        _userProfileRepository = userProfileRepository;
        _garmentRepository = garmentRepository;
        _renderCacheRepository = renderCacheRepository;
        _aiPipeline = aiPipeline;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<UserPhotoDto> AddPhotoAsync(AddTryOnPhotoDto input)
    {
        var clean = await _aiPipeline.CheckNsfwAsync(input.PhotoUrl);
        if (!clean)
        {
            throw new UserFriendlyException("Bu görsel uygun bulunmadı.");
        }

        var analysis = await _aiPipeline.AnalyzeUserPhotoAsync(input.PhotoUrl);
        var photo = new UserPhoto(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            input.PhotoUrl,
            CurrentTenant.Id,
            analysis?.QualityScore,
            analysis?.HasFace ?? false,
            analysis?.IsFullBody ?? false);

        await _userPhotoRepository.InsertAsync(photo, autoSave: true);
        return ObjectMapper.Map<UserPhoto, UserPhotoDto>(photo);
    }

    public virtual async Task<List<UserPhotoDto>> GetPhotosAsync()
    {
        var userId = CurrentUser.GetId();
        var q = (await _userPhotoRepository.GetQueryableAsync()).Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreationTime);
        var list = await _asyncExecuter.ToListAsync(q);
        return list.Select(p => ObjectMapper.Map<UserPhoto, UserPhotoDto>(p)).ToList();
    }

    public virtual async Task DeletePhotoAsync(Guid id)
    {
        var photo = await _userPhotoRepository.GetAsync(id);
        if (photo.UserId != CurrentUser.GetId())
        {
            throw new BusinessException(OVODomainErrorCodes.GarmentAccessDenied);
        }

        await _userPhotoRepository.DeleteAsync(photo, autoSave: true);
    }

    public virtual async Task<StudioPhotoDto> GetStudioPhotoAsync()
    {
        var profile = await GetOrCreateProfileAsync();
        return new StudioPhotoDto { StudioPhotoUrl = profile.StudioPhotoUrl };
    }

    public virtual async Task SetStudioPhotoAsync(SetStudioPhotoDto input)
    {
        var profile = await GetOrCreateProfileAsync();
        if (input.RegenerateFromPhotos && input.SourcePhotoUrls is { Count: > 0 })
        {
            profile.StudioPhotoUrl = await _aiPipeline.GenerateStudioPhotoAsync(input.SourcePhotoUrls);
        }
        else if (input.StudioPhotoUrl != null)
        {
            profile.StudioPhotoUrl = input.StudioPhotoUrl;
        }

        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public virtual async Task<TryOnRenderResultDto> RenderAsync(TryOnRenderInputDto input)
    {
        if (input.GarmentIds.Count < MinGarmentCount)
        {
            throw new UserFriendlyException($"Try-on için en az {MinGarmentCount} kıyafet seçmelisiniz.");
        }

        var userId = CurrentUser.GetId();
        var profile = await GetOrCreateProfileAsync();
        EnsureActiveAccount(profile);

        if (string.IsNullOrEmpty(profile.StudioPhotoUrl))
        {
            throw new UserFriendlyException("Önce stüdyo fotoğrafı oluşturun veya yükleyin.");
        }

        await EnsureGarmentsOwnedAsync(input.GarmentIds);

        var hash = TryOnHashHelper.BuildComboHash(userId, input.GarmentIds);
        var cached = await _renderCacheRepository.FindAsync(hash);
        if (cached != null)
        {
            return new TryOnRenderResultDto { ComboHash = hash, RenderUrl = cached.RenderUrl };
        }

        var cutouts = new List<string>();
        foreach (var gid in input.GarmentIds.Distinct())
        {
            var g = await _garmentRepository.GetAsync(gid);
            cutouts.Add(g.CutoutImageUrl);
        }

        await EnsureRenderQuotaAsync(profile);

        var renderUrl = await _aiPipeline.RenderTryOnAsync(profile.StudioPhotoUrl, cutouts);
        var entry = new RenderCache(hash, renderUrl, CurrentTenant.Id);
        await _renderCacheRepository.InsertAsync(entry, autoSave: true);

        profile.DailyRenderCount++;
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);

        return new TryOnRenderResultDto { ComboHash = hash, RenderUrl = renderUrl };
    }

    public virtual async Task<TryOnRenderResultDto> GetRenderAsync(string comboHash)
    {
        var cached = await _renderCacheRepository.GetAsync(comboHash);
        return new TryOnRenderResultDto { ComboHash = cached.Id, RenderUrl = cached.RenderUrl };
    }

    private async Task<UserProfile> GetOrCreateProfileAsync()
    {
        var userId = CurrentUser.GetId();
        var profile = await _userProfileRepository.FindAsync(userId);
        if (profile != null)
        {
            return profile;
        }

        profile = new UserProfile(userId) { TenantId = CurrentTenant.Id };
        await _userProfileRepository.InsertAsync(profile, autoSave: true);
        return profile;
    }

    private static void EnsureActiveAccount(UserProfile profile)
    {
        if (profile.AccountStatus == AccountStatus.Frozen)
        {
            throw new BusinessException(OVODomainErrorCodes.AccountFrozen);
        }
    }

    private async Task EnsureGarmentsOwnedAsync(IReadOnlyList<Guid> garmentIds)
    {
        foreach (var gid in garmentIds.Distinct())
        {
            var g = await _garmentRepository.GetAsync(gid);
            if (g.UserId != CurrentUser.GetId())
            {
                throw new BusinessException(OVODomainErrorCodes.GarmentAccessDenied).WithData("GarmentId", gid);
            }
        }
    }

    private async Task EnsureRenderQuotaAsync(UserProfile profile)
    {
        if (profile.Package == UserPackage.Premium)
        {
            return;
        }

        var now = Clock.Now;
        var reset = false;
        if (!profile.DailyRenderCountResetAt.HasValue ||
            (now - profile.DailyRenderCountResetAt.Value).TotalHours >= 24)
        {
            profile.DailyRenderCount = 0;
            profile.DailyRenderCountResetAt = now;
            reset = true;
        }

        if (profile.DailyRenderCount >= FreeDailyRenderLimit)
        {
            throw new BusinessException(OVODomainErrorCodes.RenderDailyLimitExceeded);
        }

        if (reset)
        {
            await _userProfileRepository.UpdateAsync(profile, autoSave: true);
        }
    }
}
