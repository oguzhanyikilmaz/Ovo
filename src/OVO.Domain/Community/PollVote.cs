using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Community;

public class PollVote : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid PollId { get; set; }

    public Guid UserId { get; set; }

    public int OptionIndex { get; set; }

    protected PollVote()
    {
    }

    public PollVote(Guid id, Guid pollId, Guid userId, int optionIndex, Guid? tenantId = null)
        : base(id)
    {
        PollId = pollId;
        UserId = userId;
        OptionIndex = optionIndex;
        TenantId = tenantId;
    }
}
