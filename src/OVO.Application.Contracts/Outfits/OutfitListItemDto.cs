using System;
using System.Collections.Generic;
using OVO.Outfits;
using Volo.Abp.Application.Dtos;

namespace OVO.Outfits;

public class OutfitListItemDto : CreationAuditedEntityDto<Guid>
{
    public List<Guid> GarmentIds { get; set; } = new();

    public string? RenderUrl { get; set; }

    public OutfitVisibility Visibility { get; set; }
}
