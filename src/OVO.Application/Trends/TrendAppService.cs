using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO.Outfits;
using OVO.Permissions;
using OVO.Wardrobe;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace OVO.Trends;

[Authorize(OVOPermissions.Trends.Default)]
public class TrendAppService : OVOAppService, ITrendAppService
{
    private readonly IGarmentRepository _garmentRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public TrendAppService(
        IGarmentRepository garmentRepository,
        IOutfitRepository outfitRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _garmentRepository = garmentRepository;
        _outfitRepository = outfitRepository;
        _asyncExecuter = asyncExecuter;
    }

    [AllowAnonymous]
    public virtual async Task<List<TrendColorDto>> GetColorsAsync()
    {
        var q = await _garmentRepository.GetQueryableAsync();
        var colors = await _asyncExecuter.ToListAsync(
            q.Where(g => g.Visibility == GarmentVisibility.Visible).Select(g => g.Color));
        if (colors.Count == 0)
        {
            return new List<TrendColorDto>();
        }

        var total = colors.Count;
        return colors
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .Take(16)
            .Select(g => new TrendColorDto
            {
                Color = g.Key,
                Count = g.Count(),
                Percent = Math.Round(100.0 * g.Count() / total, 1)
            })
            .ToList();
    }

    [AllowAnonymous]
    public virtual async Task<PagedResultDto<TrendOutfitDto>> GetOutfitsAsync(GetTrendOutfitsInput input)
    {
        var q = (await _outfitRepository.GetQueryableAsync())
            .Where(o => o.IsShared && o.Visibility == OutfitVisibility.Visible)
            .OrderByDescending(o => o.CreationTime);
        var total = await _asyncExecuter.CountAsync(q);
        var items = await _asyncExecuter.ToListAsync(q.PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<TrendOutfitDto>(
            total,
            items.Select(o => new TrendOutfitDto
            {
                OutfitId = o.Id,
                RenderUrl = o.RenderUrl,
                LikeScore = (int)(o.HarmonyScore ?? 0)
            }).ToList());
    }
}
