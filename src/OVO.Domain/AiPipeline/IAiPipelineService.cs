using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OVO.AiPipeline;

public interface IAiPipelineService
{
    Task<bool> CheckNsfwAsync(string imageUrl, CancellationToken cancellationToken = default);

    Task<GarmentImageProcessResult> ProcessGarmentImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default);

    Task<string?> ExtractProductImageFromUrlAsync(string pageUrl, CancellationToken cancellationToken = default);

    Task<string> GenerateStudioPhotoAsync(
        IReadOnlyList<string> sourcePhotoUrls,
        CancellationToken cancellationToken = default);

    Task<string> RenderTryOnAsync(
        string studioPhotoUrl,
        IReadOnlyList<string> garmentCutoutUrls,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> SuggestOutfitGarmentIdsAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> AutoCompleteOutfitAsync(
        IReadOnlyList<Guid> selectedGarmentIds,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UserPhotoAnalysisResult?> AnalyzeUserPhotoAsync(
        string photoUrl,
        CancellationToken cancellationToken = default);
}
