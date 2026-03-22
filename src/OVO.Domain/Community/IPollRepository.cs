using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Community;

public interface IPollRepository : IRepository<Poll, Guid>
{
}
