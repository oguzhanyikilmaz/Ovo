using System;
using Volo.Abp.Domain.Repositories;

namespace OVO.Moderation;

public interface IContentReportRepository : IRepository<ContentReport, Guid>
{
}
