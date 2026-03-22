using System;
using OVO.EntityFrameworkCore;
using OVO.Moderation;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore.Moderation;

public class ContentReportRepository : EfCoreRepository<OVODbContext, ContentReport, Guid>, IContentReportRepository
{
    public ContentReportRepository(IDbContextProvider<OVODbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
}
