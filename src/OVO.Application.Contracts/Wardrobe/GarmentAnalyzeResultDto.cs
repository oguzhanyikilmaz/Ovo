namespace OVO.Wardrobe;

public class GarmentAnalyzeResultDto
{
    public GarmentCategory Category { get; set; }

    public string SubCategory { get; set; } = default!;

    public string Color { get; set; } = default!;

    public string Pattern { get; set; } = default!;

    public string Seasons { get; set; } = default!;

    public GarmentFormality Formality { get; set; }
}
