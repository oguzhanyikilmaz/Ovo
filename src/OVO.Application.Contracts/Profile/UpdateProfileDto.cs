using OVO.Users;

namespace OVO.Profile;

public class UpdateProfileDto
{
    public string? Gender { get; set; }

    public string? StudioPhotoUrl { get; set; }

    public double? HeightCm { get; set; }

    public double? WeightKg { get; set; }

    public string? BodyType { get; set; }

    public UserPackage? Package { get; set; }
}
