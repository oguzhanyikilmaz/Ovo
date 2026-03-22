using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Social;

public class UserFollow : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid FollowerId { get; set; }

    public Guid FollowingId { get; set; }

    protected UserFollow()
    {
    }

    public UserFollow(Guid id, Guid followerId, Guid followingId, Guid? tenantId = null)
        : base(id)
    {
        FollowerId = followerId;
        FollowingId = followingId;
        TenantId = tenantId;
    }
}
