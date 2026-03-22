namespace OVO.Wardrobe;

public class UpdateGarmentDto
{
    public GarmentCategory? Category { get; set; }

    public string? SubCategory { get; set; }

    public string? Color { get; set; }

    public string? Pattern { get; set; }

    public string? Seasons { get; set; }

    public GarmentFormality? Formality { get; set; }

    public string? Size { get; set; }

    public GarmentVisibility? Visibility { get; set; }

    public string? Notes { get; set; }
}
