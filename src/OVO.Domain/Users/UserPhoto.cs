using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Users;

public class UserPhoto : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    public string PhotoUrl { get; set; } = default!;

    public double? QualityScore { get; set; }

    public bool HasFace { get; set; }

    public bool IsFullBody { get; set; }

    protected UserPhoto()
    {
    }

    public UserPhoto(
        Guid id,
        Guid userId,
        string photoUrl,
        Guid? tenantId = null,
        double? qualityScore = null,
        bool hasFace = false,
        bool isFullBody = false)
        : base(id)
    {
        UserId = userId;
        TenantId = tenantId;
        PhotoUrl = photoUrl;
        QualityScore = qualityScore;
        HasFace = hasFace;
        IsFullBody = isFullBody;
    }
}
