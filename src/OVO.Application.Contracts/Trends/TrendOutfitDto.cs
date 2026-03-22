using System;

namespace OVO.Trends;

public class TrendOutfitDto
{
    public Guid OutfitId { get; set; }

    public string? RenderUrl { get; set; }

    public int LikeScore { get; set; }
}
