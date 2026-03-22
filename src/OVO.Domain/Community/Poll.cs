using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Community;

public class Poll : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid CreatorUserId { get; set; }

    public Guid? TenantId { get; set; }

    public string Question { get; set; } = default!;

    /// <summary>Seçenekler JSON (label, imageUrl vb.).</summary>
    public string OptionsJson { get; set; } = default!;

    /// <summary>Her seçenek için oy sayıları [n0,n1,...].</summary>
    public string OptionVoteCountsJson { get; set; } = default!;

    public int TotalVotes { get; set; }

    public bool IsPublished { get; set; }

    protected Poll()
    {
    }

    public Poll(
        Guid id,
        Guid creatorUserId,
        string question,
        string optionsJson,
        string optionVoteCountsJson,
        bool isPublished,
        Guid? tenantId = null)
        : base(id)
    {
        CreatorUserId = creatorUserId;
        TenantId = tenantId;
        Question = question;
        OptionsJson = optionsJson;
        OptionVoteCountsJson = optionVoteCountsJson;
        IsPublished = isPublished;
    }
}
