using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Wardrobe;

public class Garment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    public GarmentCategory Category { get; set; }

    public string SubCategory { get; set; } = default!;

    public string Color { get; set; } = default!;

    public string Pattern { get; set; } = default!;

    /// <summary>Çoklu mevsim: virgülle ayrılmış (örn. İlkbahar,Yaz).</summary>
    public string Seasons { get; set; } = default!;

    public GarmentFormality Formality { get; set; }

    public string? Size { get; set; }

    public GarmentVisibility Visibility { get; set; }

    public GarmentSource Source { get; set; }

    public string? Notes { get; set; }

    public string OriginalImageUrl { get; set; } = default!;

    public string CutoutImageUrl { get; set; } = default!;

    protected Garment()
    {
    }

    public Garment(
        Guid id,
        Guid userId,
        GarmentCategory category,
        string subCategory,
        string color,
        string pattern,
        string seasons,
        GarmentFormality formality,
        GarmentVisibility visibility,
        GarmentSource source,
        string originalImageUrl,
        string cutoutImageUrl,
        string? size = null,
        string? notes = null,
        Guid? tenantId = null)
        : base(id)
    {
        UserId = userId;
        TenantId = tenantId;
        Category = category;
        SubCategory = subCategory;
        Color = color;
        Pattern = pattern;
        Seasons = seasons;
        Formality = formality;
        Visibility = visibility;
        Source = source;
        OriginalImageUrl = originalImageUrl;
        CutoutImageUrl = cutoutImageUrl;
        Size = size;
        Notes = notes;
    }
}
