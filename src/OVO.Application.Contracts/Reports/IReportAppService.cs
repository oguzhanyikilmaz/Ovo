using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.Reports;

public interface IReportAppService : IApplicationService
{
    Task<ReportDto> CreateAsync(CreateReportDto input);
}
