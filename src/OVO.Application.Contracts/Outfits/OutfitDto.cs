using System;
using System.Collections.Generic;
using OVO.Outfits;
using Volo.Abp.Application.Dtos;

namespace OVO.Outfits;

public class OutfitDto : FullAuditedEntityDto<Guid>
{
    public Guid UserId { get; set; }

    public List<Guid> GarmentIds { get; set; } = new();

    public string? ComboHash { get; set; }

    public decimal? HarmonyScore { get; set; }

    public string? RenderUrl { get; set; }

    public OutfitVisibility Visibility { get; set; }

    public bool IsShared { get; set; }
}
