using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.Social;

public interface ISocialAppService : IApplicationService
{
    Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId);

    Task FollowAsync(Guid userId);

    Task UnfollowAsync(Guid userId);

    Task BlockAsync(Guid userId);

    Task UnblockAsync(Guid userId);
}
