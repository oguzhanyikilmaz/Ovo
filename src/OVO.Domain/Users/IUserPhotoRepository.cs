using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Users;

public interface IUserPhotoRepository : IRepository<UserPhoto, Guid>
{
}
