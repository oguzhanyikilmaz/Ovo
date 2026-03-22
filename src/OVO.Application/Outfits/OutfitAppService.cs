using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO;
using OVO.AiPipeline;
using OVO.Internal;
using OVO.Permissions;
using OVO.Wardrobe;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace OVO.Outfits;

[Authorize(OVOPermissions.Outfits.Default)]
public class OutfitAppService : OVOAppService, IOutfitAppService
{
    private readonly IOutfitRepository _outfitRepository;
    private readonly IGarmentRepository _garmentRepository;
    private readonly IAiPipelineService _aiPipeline;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public OutfitAppService(
        IOutfitRepository outfitRepository,
        IGarmentRepository garmentRepository,
        IAiPipelineService aiPipeline,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _outfitRepository = outfitRepository;
        _garmentRepository = garmentRepository;
        _aiPipeline = aiPipeline;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<OutfitDto> GetDailyAsync()
    {
        var userId = CurrentUser.GetId();
        var query = (await _outfitRepository.GetQueryableAsync())
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreationTime);
        var list = await _asyncExecuter.ToListAsync(query.Take(1));
        var outfit = list.FirstOrDefault();
        if (outfit == null)
        {
            throw new UserFriendlyException("Önce dolabına kıyafet ve kombin ekle.");
        }

        return MapToOutfitDto(outfit);
    }

    public virtual async Task<List<OutfitSuggestionDto>> GetSuggestionsAsync()
    {
        var userId = CurrentUser.GetId();
        var suggestions = new List<OutfitSuggestionDto>();
        for (var i = 0; i < 10; i++)
        {
            var ids = await _aiPipeline.SuggestOutfitGarmentIdsAsync(userId, 4);
            suggestions.Add(new OutfitSuggestionDto
            {
                GarmentIds = ids.ToList(),
                HarmonyScore = 0.5m + (decimal)(i * 0.03)
            });
        }

        return suggestions;
    }

    public virtual async Task<PagedResultDto<OutfitListItemDto>> GetListAsync(GetOutfitListInput input)
    {
        var userId = CurrentUser.GetId();
        var query = (await _outfitRepository.GetQueryableAsync()).Where(o => o.UserId == userId);
        query = query.OrderByDescending(o => o.CreationTime);
        var total = await _asyncExecuter.CountAsync(query);
        var items = await _asyncExecuter.ToListAsync(query.PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<OutfitListItemDto>(total, items.Select(MapToListItemDto).ToList());
    }

    public virtual async Task<OutfitDto> GetAsync(Guid id)
    {
        var outfit = await _outfitRepository.GetAsync(id);
        EnsureOwner(outfit);
        return MapToOutfitDto(outfit);
    }

    public virtual async Task<OutfitDto> CreateAsync(CreateOutfitDto input)
    {
        await EnsureGarmentsOwnedAsync(input.GarmentIds);
        var json = OvoSerialization.GarmentIdsToJson(input.GarmentIds);
        var outfit = new Outfit(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            json,
            input.Visibility,
            CurrentTenant.Id,
            input.ComboHash,
            input.HarmonyScore,
            input.RenderUrl);
        await _outfitRepository.InsertAsync(outfit, autoSave: true);
        return MapToOutfitDto(outfit);
    }

    public virtual async Task<OutfitDto> UpdateAsync(Guid id, UpdateOutfitDto input)
    {
        var outfit = await _outfitRepository.GetAsync(id);
        EnsureOwner(outfit);
        if (input.GarmentIds != null)
        {
            await EnsureGarmentsOwnedAsync(input.GarmentIds);
            outfit.GarmentIdsJson = OvoSerialization.GarmentIdsToJson(input.GarmentIds);
        }

        if (input.Visibility.HasValue)
        {
            outfit.Visibility = input.Visibility.Value;
        }

        if (input.RenderUrl != null)
        {
            outfit.RenderUrl = input.RenderUrl;
        }

        await _outfitRepository.UpdateAsync(outfit, autoSave: true);
        return MapToOutfitDto(outfit);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var outfit = await _outfitRepository.GetAsync(id);
        EnsureOwner(outfit);
        await _outfitRepository.DeleteAsync(outfit, autoSave: true);
    }

    public virtual async Task<AutoCompleteOutfitResultDto> AutoCompleteAsync(AutoCompleteOutfitInputDto input)
    {
        var ids = await _aiPipeline.AutoCompleteOutfitAsync(input.SelectedGarmentIds, CurrentUser.GetId());
        return new AutoCompleteOutfitResultDto { GarmentIds = ids.ToList() };
    }

    public virtual async Task ShareAsync(Guid id)
    {
        var outfit = await _outfitRepository.GetAsync(id);
        EnsureOwner(outfit);
        outfit.IsShared = true;
        outfit.Visibility = OutfitVisibility.Visible;
        await _outfitRepository.UpdateAsync(outfit, autoSave: true);
    }

    private void EnsureOwner(Outfit outfit)
    {
        if (outfit.UserId != CurrentUser.GetId())
        {
            throw new BusinessException(OVODomainErrorCodes.OutfitAccessDenied).WithData("OutfitId", outfit.Id);
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

    private OutfitDto MapToOutfitDto(Outfit outfit)
    {
        return new OutfitDto
        {
            Id = outfit.Id,
            UserId = outfit.UserId,
            GarmentIds = OvoSerialization.GarmentIdsFromJson(outfit.GarmentIdsJson),
            ComboHash = outfit.ComboHash,
            HarmonyScore = outfit.HarmonyScore,
            RenderUrl = outfit.RenderUrl,
            Visibility = outfit.Visibility,
            IsShared = outfit.IsShared,
            CreationTime = outfit.CreationTime,
            CreatorId = outfit.CreatorId,
            LastModificationTime = outfit.LastModificationTime,
            LastModifierId = outfit.LastModifierId
        };
    }

    private OutfitListItemDto MapToListItemDto(Outfit outfit)
    {
        return new OutfitListItemDto
        {
            Id = outfit.Id,
            GarmentIds = OvoSerialization.GarmentIdsFromJson(outfit.GarmentIdsJson),
            RenderUrl = outfit.RenderUrl,
            Visibility = outfit.Visibility,
            CreationTime = outfit.CreationTime,
            CreatorId = outfit.CreatorId
        };
    }
}
