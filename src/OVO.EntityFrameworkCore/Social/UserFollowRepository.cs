using System;
using OVO.EntityFrameworkCore;
using OVO.Social;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Social;

public class UserFollowRepository : EfCoreRepository<OVODbContext, UserFollow, Guid>, IUserFollowRepository
{
    public UserFollowRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
