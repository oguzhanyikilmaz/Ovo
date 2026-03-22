using System;
using OVO.EntityFrameworkCore;
using OVO.Users;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Users;

public class UserPhotoRepository : EfCoreRepository<OVODbContext, UserPhoto, Guid>, IUserPhotoRepository
{
    public UserPhotoRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
