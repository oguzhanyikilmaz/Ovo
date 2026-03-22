using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Community;

public interface IPollVoteRepository : IRepository<PollVote, Guid>
{
}
