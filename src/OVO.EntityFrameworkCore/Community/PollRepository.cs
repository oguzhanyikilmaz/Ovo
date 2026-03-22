using System;
using OVO.EntityFrameworkCore;
using OVO.Community;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Community;

public class PollRepository : EfCoreRepository<OVODbContext, Poll, Guid>, IPollRepository
{
    public PollRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
