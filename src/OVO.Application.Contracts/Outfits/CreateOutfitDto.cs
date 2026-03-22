using System;
using System.Collections.Generic;

namespace OVO.Outfits;

public class CreateOutfitDto
{
    public List<Guid> GarmentIds { get; set; } = new();

    public OutfitVisibility Visibility { get; set; } = OutfitVisibility.Visible;

    public string? RenderUrl { get; set; }

    public decimal? HarmonyScore { get; set; }

    public string? ComboHash { get; set; }
}
