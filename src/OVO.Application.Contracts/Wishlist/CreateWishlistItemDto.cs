namespace OVO.Wishlist;

public class CreateWishlistItemDto
{
    public WishlistContentType ContentType { get; set; }

    public string ContentId { get; set; } = default!;

    public string? SourceType { get; set; }

    public string? SourceLabel { get; set; }

    public string? PreviewImageUrl { get; set; }
}
