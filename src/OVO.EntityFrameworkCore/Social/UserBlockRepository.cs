using System;
using OVO.EntityFrameworkCore;
using OVO.Social;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Social;

public class UserBlockRepository : EfCoreRepository<OVODbContext, UserBlock, Guid>, IUserBlockRepository
{
    public UserBlockRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
