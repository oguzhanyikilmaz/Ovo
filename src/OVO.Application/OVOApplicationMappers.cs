using OVO.Moderation;
using OVO.Profile;
using OVO.Reports;
using OVO.TryOn;
using OVO.Users;
using OVO.Wardrobe;
using OVO.Wishlist;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace OVO;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class OVOApplicationMappers
{
    public partial GarmentDto GarmentToGarmentDto(Garment garment);

    public partial GarmentListItemDto GarmentToGarmentListItemDto(Garment garment);

    public partial ProfileDto UserProfileToProfileDto(UserProfile profile);

    public partial UserPhotoDto UserPhotoToUserPhotoDto(UserPhoto photo);

    public partial WishlistItemDto WishlistItemToWishlistItemDto(WishlistItem item);

    public partial ReportDto ContentReportToReportDto(ContentReport report);
}
