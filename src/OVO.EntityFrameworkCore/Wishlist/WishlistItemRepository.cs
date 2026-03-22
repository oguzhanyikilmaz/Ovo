using System;
using OVO.EntityFrameworkCore;
using OVO.Wishlist;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Wishlist;

public class WishlistItemRepository : EfCoreRepository<OVODbContext, WishlistItem, Guid>, IWishlistItemRepository
{
    public WishlistItemRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
