using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Social;

public interface IUserBlockRepository : IRepository<UserBlock, Guid>
{
}
