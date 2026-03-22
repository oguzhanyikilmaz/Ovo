using System;
using OVO.Moderation;
using Volo.Abp.Application.Dtos;

namespace OVO.Reports;

public class ReportDto : CreationAuditedEntityDto<Guid>
{
    public string ContentType { get; set; } = default!;

    public string ContentId { get; set; } = default!;

    public string Reason { get; set; } = default!;

    public ReportStatus Status { get; set; }
}
