using System;
using OVO.Users;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace OVO.Users;

/// <summary>Kimlik kullanıcısı ile bire bir; <see cref="AuditedAggregateRoot{Guid}.Id"/> = IdentityUser.Id.</summary>
public class UserProfile : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public string? Gender { get; set; }

    public string? StudioPhotoUrl { get; set; }

    public UserPackage Package { get; set; }

    public int DailyRenderCount { get; set; }

    public DateTime? DailyRenderCountResetAt { get; set; }

    public double? HeightCm { get; set; }

    public double? WeightKg { get; set; }

    public string? BodyType { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public DateTime? DeletionRequestedAt { get; set; }

    protected UserProfile()
    {
    }

    public UserProfile(Guid id) : base(id)
    {
        Package = UserPackage.Free;
        AccountStatus = AccountStatus.Active;
    }
}
