using System;
using OVO.EntityFrameworkCore;
using OVO.Outfits;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Outfits;

public class OutfitRepository : EfCoreRepository<OVODbContext, Outfit, Guid>, IOutfitRepository
{
    public OutfitRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
