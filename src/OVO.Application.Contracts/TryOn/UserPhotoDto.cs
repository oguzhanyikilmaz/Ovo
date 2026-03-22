using System;
using Volo.Abp.Application.Dtos;

namespace OVO.TryOn;

public class UserPhotoDto : CreationAuditedEntityDto<Guid>
{
    public string PhotoUrl { get; set; } = default!;

    public double? QualityScore { get; set; }

    public bool HasFace { get; set; }

    public bool IsFullBody { get; set; }
}
