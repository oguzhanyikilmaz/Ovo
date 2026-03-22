using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Outfits;

public interface IOutfitRepository : IRepository<Outfit, Guid>
{
}
