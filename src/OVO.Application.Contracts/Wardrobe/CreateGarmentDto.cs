namespace OVO.Wardrobe;

public class CreateGarmentDto
{
    public string OriginalImageUrl { get; set; } = default!;

    public GarmentSource Source { get; set; } = GarmentSource.Upload;

    public GarmentCategory? Category { get; set; }

    public string? SubCategory { get; set; }

    public string? Color { get; set; }

    public string? Pattern { get; set; }

    public string? Seasons { get; set; }

    public GarmentFormality? Formality { get; set; }

    public string? Size { get; set; }

    public GarmentVisibility Visibility { get; set; } = GarmentVisibility.Visible;

    public string? Notes { get; set; }
}
