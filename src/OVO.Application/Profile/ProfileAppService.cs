using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO.Permissions;
using OVO.Users;
using Volo.Abp;
using Volo.Abp.Users;

namespace OVO.Profile;

[Authorize(OVOPermissions.Profile.Default)]
public class ProfileAppService : OVOAppService, IProfileAppService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public ProfileAppService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public virtual async Task<ProfileDto> GetAsync()
    {
        var profile = await GetOrCreateProfileAsync();
        return ObjectMapper.Map<UserProfile, ProfileDto>(profile);
    }

    public virtual async Task<ProfileDto> UpdateAsync(UpdateProfileDto input)
    {
        var profile = await GetOrCreateProfileAsync();
        if (input.Gender != null)
        {
            profile.Gender = input.Gender;
        }

        if (input.StudioPhotoUrl != null)
        {
            profile.StudioPhotoUrl = input.StudioPhotoUrl;
        }

        if (input.HeightCm.HasValue)
        {
            profile.HeightCm = input.HeightCm;
        }

        if (input.WeightKg.HasValue)
        {
            profile.WeightKg = input.WeightKg;
        }

        if (input.BodyType != null)
        {
            profile.BodyType = input.BodyType;
        }

        if (input.Package.HasValue)
        {
            profile.Package = input.Package.Value;
        }

        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
        return ObjectMapper.Map<UserProfile, ProfileDto>(profile);
    }

    public virtual async Task FreezeAsync()
    {
        var profile = await GetOrCreateProfileAsync();
        profile.AccountStatus = AccountStatus.Frozen;
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public virtual async Task RequestDeletionAsync()
    {
        var profile = await GetOrCreateProfileAsync();
        profile.AccountStatus = AccountStatus.PendingDeletion;
        profile.DeletionRequestedAt = Clock.Now;
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public virtual async Task CancelDeletionAsync()
    {
        var profile = await GetOrCreateProfileAsync();
        profile.AccountStatus = AccountStatus.Active;
        profile.DeletionRequestedAt = null;
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public virtual async Task<ProfileExportDto> ExportAsync()
    {
        var profile = await GetOrCreateProfileAsync();
        var dto = ObjectMapper.Map<UserProfile, ProfileDto>(profile);
        var json = JsonSerializer.Serialize(dto);
        return new ProfileExportDto { JsonPayload = json };
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
}
