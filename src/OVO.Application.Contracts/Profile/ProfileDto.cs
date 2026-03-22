using System;
using OVO.Users;
using Volo.Abp.Application.Dtos;

namespace OVO.Profile;

public class ProfileDto : FullAuditedEntityDto<Guid>
{
    public string? Gender { get; set; }

    public string? StudioPhotoUrl { get; set; }

    public UserPackage Package { get; set; }

    public int DailyRenderCount { get; set; }

    public double? HeightCm { get; set; }

    public double? WeightKg { get; set; }

    public string? BodyType { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public DateTime? DeletionRequestedAt { get; set; }
}
