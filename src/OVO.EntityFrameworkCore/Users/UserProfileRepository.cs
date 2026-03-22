using System;
using OVO.EntityFrameworkCore;
using OVO.Users;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Users;

public class UserProfileRepository : EfCoreRepository<OVODbContext, UserProfile, Guid>, IUserProfileRepository
{
    public UserProfileRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
