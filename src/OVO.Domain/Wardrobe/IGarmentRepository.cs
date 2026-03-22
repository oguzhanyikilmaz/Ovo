using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Wardrobe;

public interface IGarmentRepository : IRepository<Garment, Guid>
{
}
