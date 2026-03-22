using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace OVO.Wardrobe;

public interface IWardrobeAppService : IApplicationService
{
    Task<GarmentDto> GetAsync(Guid id);

    Task<PagedResultDto<GarmentListItemDto>> GetListAsync(GetGarmentListInput input);

    Task<GarmentDto> CreateAsync(CreateGarmentDto input);

    Task<GarmentDto> UpdateAsync(Guid id, UpdateGarmentDto input);

    Task DeleteAsync(Guid id);

    Task<GarmentDto> CreateFromUrlAsync(CreateGarmentFromUrlDto input);

    Task<GarmentDto> CreateFromPhotoAsync(CreateGarmentFromPhotoDto input);

    Task<GarmentAnalyzeResultDto> AnalyzeImageAsync(GarmentAnalyzeInputDto input);

    Task<WardrobeInsightsDto> GetInsightsAsync();
}
