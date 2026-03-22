using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OVO.Similar;

[AllowAnonymous]
public class SimilarAppService : OVOAppService, ISimilarAppService
{
    public virtual Task<SimilarSearchJobDto> SearchAsync(SimilarSearchInputDto input)
    {
        var results = new List<SimilarProductDto>
        {
            new()
            {
                Vendor = "Demo",
                Title = "Benzer ürün (stub)",
                PriceTry = 599m,
                Url = "https://example.com",
                Similarity = 0.91
            }
        };
        var jobId = SimilarSearchStore.CreateJob(results);
        return Task.FromResult(new SimilarSearchJobDto { JobId = jobId });
    }

    public virtual Task<List<SimilarProductDto>> GetResultsAsync(string jobId)
    {
        return Task.FromResult(SimilarSearchStore.GetResults(jobId));
    }
}
