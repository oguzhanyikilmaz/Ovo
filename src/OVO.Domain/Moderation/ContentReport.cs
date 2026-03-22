using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Moderation;

public class ContentReport : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid ReporterUserId { get; set; }

    public Guid? TenantId { get; set; }

    public string ContentType { get; set; } = default!;

    public string ContentId { get; set; } = default!;

    public string Reason { get; set; } = default!;

    public ReportStatus Status { get; set; }

    protected ContentReport()
    {
    }

    public ContentReport(
        Guid id,
        Guid reporterUserId,
        string contentType,
        string contentId,
        string reason,
        Guid? tenantId = null)
        : base(id)
    {
        ReporterUserId = reporterUserId;
        TenantId = tenantId;
        ContentType = contentType;
        ContentId = contentId;
        Reason = reason;
        Status = ReportStatus.Pending;
    }
}
