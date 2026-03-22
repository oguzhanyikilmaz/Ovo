using System;
using System.Collections.Generic;

namespace OVO.Outfits;

public class OutfitSuggestionDto
{
    public List<Guid> GarmentIds { get; set; } = new();

    public decimal? HarmonyScore { get; set; }
}
