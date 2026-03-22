using System;

namespace OVO.Social;

public class PublicUserProfileDto
{
    public Guid UserId { get; set; }

    public string? UserName { get; set; }

    public string? StudioPhotoUrl { get; set; }

    public int FollowerCount { get; set; }

    public int FollowingCount { get; set; }

    public int VisibleOutfitCount { get; set; }
}
