namespace OVO.Wardrobe;

public class CreateGarmentFromUrlDto
{
    public string PageUrl { get; set; } = default!;

    public GarmentVisibility Visibility { get; set; } = GarmentVisibility.Visible;

    public string? Size { get; set; }

    public string? Notes { get; set; }
}
