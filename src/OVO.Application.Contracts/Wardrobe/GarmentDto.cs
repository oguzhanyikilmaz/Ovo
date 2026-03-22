using System;
using Volo.Abp.Application.Dtos;

namespace OVO.Wardrobe;

public class GarmentDto : FullAuditedEntityDto<Guid>
{
    public Guid UserId { get; set; }

    public GarmentCategory Category { get; set; }

    public string SubCategory { get; set; } = default!;

    public string Color { get; set; } = default!;

    public string Pattern { get; set; } = default!;

    public string Seasons { get; set; } = default!;

    public GarmentFormality Formality { get; set; }

    public string? Size { get; set; }

    public GarmentVisibility Visibility { get; set; }

    public GarmentSource Source { get; set; }

    public string? Notes { get; set; }

    public string OriginalImageUrl { get; set; } = default!;

    public string CutoutImageUrl { get; set; } = default!;
}
