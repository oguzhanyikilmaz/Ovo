using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace OVO.Trends;

public interface ITrendAppService : IApplicationService
{
    Task<List<TrendColorDto>> GetColorsAsync();

    Task<PagedResultDto<TrendOutfitDto>> GetOutfitsAsync(GetTrendOutfitsInput input);
}
