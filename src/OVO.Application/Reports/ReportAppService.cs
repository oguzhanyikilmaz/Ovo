using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO.Moderation;
using OVO.Permissions;
using Volo.Abp.Users;

namespace OVO.Reports;

[Authorize(OVOPermissions.Reports.Create)]
public class ReportAppService : OVOAppService, IReportAppService
{
    private readonly IContentReportRepository _contentReportRepository;

    public ReportAppService(IContentReportRepository contentReportRepository)
    {
        _contentReportRepository = contentReportRepository;
    }

    public virtual async Task<ReportDto> CreateAsync(CreateReportDto input)
    {
        var report = new ContentReport(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            input.ContentType,
            input.ContentId,
            input.Reason,
            CurrentTenant.Id);

        await _contentReportRepository.InsertAsync(report, autoSave: true);
        return ObjectMapper.Map<ContentReport, ReportDto>(report);
    }
}
