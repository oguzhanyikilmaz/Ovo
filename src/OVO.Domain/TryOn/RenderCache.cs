using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace OVO.TryOn;

public class RenderCache : AggregateRoot<string>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string RenderUrl { get; set; } = default!;

    public DateTime CreationTime { get; set; }

    protected RenderCache()
    {
    }

    public RenderCache(string comboHash, string renderUrl, Guid? tenantId = null)
        : base(comboHash)
    {
        RenderUrl = renderUrl;
        TenantId = tenantId;
        CreationTime = DateTime.UtcNow;
    }
}
