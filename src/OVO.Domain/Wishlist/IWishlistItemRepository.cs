using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Wishlist;

public interface IWishlistItemRepository : IRepository<WishlistItem, Guid>
{
}
