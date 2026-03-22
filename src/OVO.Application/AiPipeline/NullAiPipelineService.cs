using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OVO.Wardrobe;

namespace OVO.AiPipeline;

public class NullAiPipelineService : IAiPipelineService
{
    public Task<bool> CheckNsfwAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public Task<GarmentImageProcessResult> ProcessGarmentImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GarmentImageProcessResult
        {
            CutoutImageUrl = imageUrl,
            Category = GarmentCategory.Top,
            SubCategory = "Gömlek",
            Color = "Beyaz",
            Pattern = "Düz",
            Seasons = "İlkbahar,Yaz",
            Formality = GarmentFormality.SmartCasual
        });
    }

    public Task<string?> ExtractProductImageFromUrlAsync(string pageUrl, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    public Task<string> GenerateStudioPhotoAsync(
        IReadOnlyList<string> sourcePhotoUrls,
        CancellationToken cancellationToken = default)
    {
        var url = sourcePhotoUrls.Count > 0
            ? sourcePhotoUrls[0]
            : "https://cdn.ovo.local/placeholder-studio.png";
        return Task.FromResult(url);
    }

    public Task<string> RenderTryOnAsync(
        string studioPhotoUrl,
        IReadOnlyList<string> garmentCutoutUrls,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(studioPhotoUrl);
    }

    public Task<IReadOnlyList<Guid>> SuggestOutfitGarmentIdsAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Guid>>(new List<Guid>());
    }

    public Task<IReadOnlyList<Guid>> AutoCompleteOutfitAsync(
        IReadOnlyList<Guid> selectedGarmentIds,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Guid>>(selectedGarmentIds.ToList());
    }

    public Task<UserPhotoAnalysisResult?> AnalyzeUserPhotoAsync(
        string photoUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<UserPhotoAnalysisResult?>(new UserPhotoAnalysisResult
        {
            QualityScore = 0.82,
            HasFace = true,
            IsFullBody = true
        });
    }
}
