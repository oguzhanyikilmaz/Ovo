using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.Similar;

public interface ISimilarAppService : IApplicationService
{
    Task<SimilarSearchJobDto> SearchAsync(SimilarSearchInputDto input);

    Task<List<SimilarProductDto>> GetResultsAsync(string jobId);
}
