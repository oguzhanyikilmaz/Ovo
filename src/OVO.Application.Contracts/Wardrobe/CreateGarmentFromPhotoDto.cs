namespace OVO.Wardrobe;

public class CreateGarmentFromPhotoDto
{
    public string ImageUrl { get; set; } = default!;

    public GarmentVisibility Visibility { get; set; } = GarmentVisibility.Visible;

    public string? Size { get; set; }

    public string? Notes { get; set; }
}
