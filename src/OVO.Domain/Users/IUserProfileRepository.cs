using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Users;

public interface IUserProfileRepository : IRepository<UserProfile, Guid>
{
}
