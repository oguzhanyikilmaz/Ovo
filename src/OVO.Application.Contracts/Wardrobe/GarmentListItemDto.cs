using System;
using Volo.Abp.Application.Dtos;

namespace OVO.Wardrobe;

public class GarmentListItemDto : CreationAuditedEntityDto<Guid>
{
    public GarmentCategory Category { get; set; }

    public string SubCategory { get; set; } = default!;

    public string Color { get; set; } = default!;

    public string CutoutImageUrl { get; set; } = default!;

    public GarmentVisibility Visibility { get; set; }
}
