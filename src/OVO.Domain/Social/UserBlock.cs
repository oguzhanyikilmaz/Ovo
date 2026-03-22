using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Social;

public class UserBlock : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid BlockerId { get; set; }

    public Guid BlockedId { get; set; }

    protected UserBlock()
    {
    }

    public UserBlock(Guid id, Guid blockerId, Guid blockedId, Guid? tenantId = null)
        : base(id)
    {
        BlockerId = blockerId;
        BlockedId = blockedId;
        TenantId = tenantId;
    }
}
