using System;
using System.IO;
using System.Threading;
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

    Task<GarmentUploadPresignResultDto> GetGarmentImageUploadPresignAsync(GarmentUploadPresignInputDto input);

    Task<GarmentDto> CreateAfterGarmentImageUploadAsync(CreateGarmentAfterClientUploadDto input);

    /// <summary>
    /// Multipart yükleme için; dinamik API dışı — <c>WardrobeUploadController</c> çağırır.
    /// </summary>
    Task<GarmentDto> CreateFromUploadStreamAsync(
        Stream fileStream,
        string fileName,
        string? contentType,
        long? contentLength,
        CreateGarmentMultipartMetadataDto metadata,
        CancellationToken cancellationToken = default);
}
