using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Wishlist;

public class WishlistItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid UserId { get; set; }

    public Guid? TenantId { get; set; }

    public WishlistContentType ContentType { get; set; }

    public string ContentId { get; set; } = default!;

    public string? SourceType { get; set; }

    public string? SourceLabel { get; set; }

    public string? PreviewImageUrl { get; set; }

    protected WishlistItem()
    {
    }

    public WishlistItem(
        Guid id,
        Guid userId,
        WishlistContentType contentType,
        string contentId,
        Guid? tenantId = null,
        string? sourceType = null,
        string? sourceLabel = null,
        string? previewImageUrl = null)
        : base(id)
    {
        UserId = userId;
        TenantId = tenantId;
        ContentType = contentType;
        ContentId = contentId;
        SourceType = sourceType;
        SourceLabel = sourceLabel;
        PreviewImageUrl = previewImageUrl;
    }
}
