using System;
using System.Collections.Generic;
using OVO.Outfits;

namespace OVO.Outfits;

public class UpdateOutfitDto
{
    public List<Guid>? GarmentIds { get; set; }

    public OutfitVisibility? Visibility { get; set; }

    public string? RenderUrl { get; set; }
}
