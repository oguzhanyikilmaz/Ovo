using OVO.EntityFrameworkCore;
using OVO.TryOn;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.TryOn;

public class RenderCacheRepository : EfCoreRepository<OVODbContext, RenderCache, string>, IRenderCacheRepository
{
    public RenderCacheRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
