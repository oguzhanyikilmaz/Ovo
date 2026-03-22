using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Social;

public interface IUserFollowRepository : IRepository<UserFollow, Guid>
{
}
