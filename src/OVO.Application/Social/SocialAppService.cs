using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO;
using OVO.Outfits;
using OVO.Permissions;
using OVO.Users;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace OVO.Social;

[Authorize(OVOPermissions.Social.Default)]
public class SocialAppService : OVOAppService, ISocialAppService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserFollowRepository _userFollowRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public SocialAppService(
        IUserProfileRepository userProfileRepository,
        IUserFollowRepository userFollowRepository,
        IUserBlockRepository userBlockRepository,
        IOutfitRepository outfitRepository,
        IIdentityUserRepository identityUserRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _userProfileRepository = userProfileRepository;
        _userFollowRepository = userFollowRepository;
        _userBlockRepository = userBlockRepository;
        _outfitRepository = outfitRepository;
        _identityUserRepository = identityUserRepository;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<PublicUserProfileDto> GetPublicProfileAsync(Guid userId)
    {
        var me = CurrentUser.Id;
        if (me.HasValue)
        {
            await EnsureNotBlockedBetweenAsync(me.Value, userId);
        }

        var user = await _identityUserRepository.FindAsync(userId)
                   ?? throw new UserFriendlyException("Kullanıcı bulunamadı.");

        var profile = await _userProfileRepository.FindAsync(userId);

        var followQ = await _userFollowRepository.GetQueryableAsync();
        var followerCount = await _asyncExecuter.CountAsync(followQ.Where(f => f.FollowingId == userId));
        var followingCount = await _asyncExecuter.CountAsync(followQ.Where(f => f.FollowerId == userId));

        var outfitQ = (await _outfitRepository.GetQueryableAsync())
            .Where(o => o.UserId == userId && o.Visibility == OutfitVisibility.Visible);
        var outfitCount = await _asyncExecuter.CountAsync(outfitQ);

        return new PublicUserProfileDto
        {
            UserId = userId,
            UserName = user.UserName,
            StudioPhotoUrl = profile?.StudioPhotoUrl,
            FollowerCount = followerCount,
            FollowingCount = followingCount,
            VisibleOutfitCount = outfitCount
        };
    }

    public virtual async Task FollowAsync(Guid userId)
    {
        var me = CurrentUser.GetId();
        if (me == userId)
        {
            throw new UserFriendlyException("Kendinizi takip edemezsiniz.");
        }

        await EnsureNotBlockedBetweenAsync(me, userId);

        var q = await _userFollowRepository.GetQueryableAsync();
        var exists = await _asyncExecuter.AnyAsync(q.Where(f => f.FollowerId == me && f.FollowingId == userId));
        if (exists)
        {
            return;
        }

        await _userFollowRepository.InsertAsync(
            new UserFollow(GuidGenerator.Create(), me, userId, CurrentTenant.Id),
            autoSave: true);
    }

    public virtual async Task UnfollowAsync(Guid userId)
    {
        var me = CurrentUser.GetId();
        var q = await _userFollowRepository.GetQueryableAsync();
        var rows = await _asyncExecuter.ToListAsync(q.Where(f => f.FollowerId == me && f.FollowingId == userId).Take(1));
        var row = rows.FirstOrDefault();
        if (row != null)
        {
            await _userFollowRepository.DeleteAsync(row, autoSave: true);
        }
    }

    public virtual async Task BlockAsync(Guid userId)
    {
        var me = CurrentUser.GetId();
        if (me == userId)
        {
            throw new UserFriendlyException("Geçersiz işlem.");
        }

        var q = await _userBlockRepository.GetQueryableAsync();
        var exists = await _asyncExecuter.AnyAsync(q.Where(b => b.BlockerId == me && b.BlockedId == userId));
        if (exists)
        {
            return;
        }

        await UnfollowAsync(userId);
        var revQ = await _userFollowRepository.GetQueryableAsync();
        var reverse = await _asyncExecuter.ToListAsync(revQ.Where(f => f.FollowerId == userId && f.FollowingId == me));
        foreach (var f in reverse)
        {
            await _userFollowRepository.DeleteAsync(f, autoSave: true);
        }

        await _userBlockRepository.InsertAsync(
            new UserBlock(GuidGenerator.Create(), me, userId, CurrentTenant.Id),
            autoSave: true);
    }

    public virtual async Task UnblockAsync(Guid userId)
    {
        var me = CurrentUser.GetId();
        var q = await _userBlockRepository.GetQueryableAsync();
        var rows = await _asyncExecuter.ToListAsync(q.Where(b => b.BlockerId == me && b.BlockedId == userId).Take(1));
        var row = rows.FirstOrDefault();
        if (row != null)
        {
            await _userBlockRepository.DeleteAsync(row, autoSave: true);
        }
    }

    private async Task EnsureNotBlockedBetweenAsync(Guid a, Guid b)
    {
        var q = await _userBlockRepository.GetQueryableAsync();
        var blocked = await _asyncExecuter.AnyAsync(
            q.Where(x => (x.BlockerId == a && x.BlockedId == b) || (x.BlockerId == b && x.BlockedId == a)));
        if (blocked)
        {
            throw new BusinessException(OVODomainErrorCodes.SocialBlockedOrInvalid);
        }
    }
}
