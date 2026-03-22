using System;
using OVO.EntityFrameworkCore;
using OVO.Wardrobe;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Wardrobe;

public class GarmentRepository : EfCoreRepository<OVODbContext, Garment, Guid>, IGarmentRepository
{
    public GarmentRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
