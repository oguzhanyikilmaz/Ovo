using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Outfits;

public class Outfit : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    /// <summary>JSON dizi: kıyafet Guid'leri.</summary>
    public string GarmentIdsJson { get; set; } = default!;

    public string? ComboHash { get; set; }

    public decimal? HarmonyScore { get; set; }

    public string? RenderUrl { get; set; }

    public OutfitVisibility Visibility { get; set; }

    public bool IsShared { get; set; }

    protected Outfit()
    {
    }

    public Outfit(
        Guid id,
        Guid userId,
        string garmentIdsJson,
        OutfitVisibility visibility,
        Guid? tenantId = null,
        string? comboHash = null,
        decimal? harmonyScore = null,
        string? renderUrl = null)
        : base(id)
    {
        UserId = userId;
        TenantId = tenantId;
        GarmentIdsJson = garmentIdsJson;
        Visibility = visibility;
        ComboHash = comboHash;
        HarmonyScore = harmonyScore;
        RenderUrl = renderUrl;
    }
}
