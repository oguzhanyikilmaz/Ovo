using System;
using OVO.EntityFrameworkCore;
using OVO.Community;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Community;

public class PollVoteRepository : EfCoreRepository<OVODbContext, PollVote, Guid>, IPollVoteRepository
{
    public PollVoteRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
