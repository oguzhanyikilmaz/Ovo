using System.Collections.Generic;

namespace OVO.TryOn;

public class SetStudioPhotoDto
{
    public string? StudioPhotoUrl { get; set; }

    public bool RegenerateFromPhotos { get; set; }

    public List<string>? SourcePhotoUrls { get; set; }
}
