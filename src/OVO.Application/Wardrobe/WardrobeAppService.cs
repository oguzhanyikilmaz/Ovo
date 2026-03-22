using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using OVO;
using OVO.AiPipeline;
using OVO.Permissions;
using OVO.Wardrobe;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace OVO.Wardrobe;

[Authorize(OVOPermissions.Wardrobe.Default)]
public class WardrobeAppService : OVOAppService, IWardrobeAppService
{
    private readonly IGarmentRepository _garmentRepository;
    private readonly IAiPipelineService _aiPipeline;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public WardrobeAppService(
        IGarmentRepository garmentRepository,
        IAiPipelineService aiPipeline,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _garmentRepository = garmentRepository;
        _aiPipeline = aiPipeline;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<GarmentDto> GetAsync(Guid id)
    {
        var garment = await _garmentRepository.GetAsync(id);
        EnsureOwner(garment);
        return ObjectMapper.Map<Garment, GarmentDto>(garment);
    }

    public virtual async Task<PagedResultDto<GarmentListItemDto>> GetListAsync(GetGarmentListInput input)
    {
        var userId = CurrentUser.GetId();
        var queryable = await _garmentRepository.GetQueryableAsync();
        var query = queryable.Where(g => g.UserId == userId);

        if (input.Category.HasValue)
        {
            query = query.Where(g => g.Category == input.Category.Value);
        }

        query = ApplySorting(query, input.Sorting);

        var totalCount = await _asyncExecuter.CountAsync(query);
        var items = await _asyncExecuter.ToListAsync(
            query.PageBy(input.SkipCount, input.MaxResultCount));

        var dtos = items.Select(g => ObjectMapper.Map<Garment, GarmentListItemDto>(g)).ToList();
        return new PagedResultDto<GarmentListItemDto>(totalCount, dtos);
    }

    public virtual async Task<GarmentDto> CreateAsync(CreateGarmentDto input)
    {
        var isClean = await _aiPipeline.CheckNsfwAsync(input.OriginalImageUrl);
        if (!isClean)
        {
            throw new UserFriendlyException("Bu görsel uygun bulunmadı.");
        }

        var processed = await _aiPipeline.ProcessGarmentImageAsync(input.OriginalImageUrl);

        var garment = new Garment(
            GuidGenerator.Create(),
            CurrentUser.GetId(),
            input.Category ?? processed.Category,
            input.SubCategory ?? processed.SubCategory,
            input.Color ?? processed.Color,
            input.Pattern ?? processed.Pattern,
            input.Seasons ?? processed.Seasons,
            input.Formality ?? processed.Formality,
            input.Visibility,
            input.Source,
            input.OriginalImageUrl,
            processed.CutoutImageUrl,
            input.Size,
            input.Notes,
            CurrentTenant.Id);

        await _garmentRepository.InsertAsync(garment, autoSave: true);
        return ObjectMapper.Map<Garment, GarmentDto>(garment);
    }

    public virtual async Task<GarmentDto> UpdateAsync(Guid id, UpdateGarmentDto input)
    {
        var garment = await _garmentRepository.GetAsync(id);
        EnsureOwner(garment);

        if (input.Category.HasValue)
        {
            garment.Category = input.Category.Value;
        }

        if (input.SubCategory != null)
        {
            garment.SubCategory = input.SubCategory;
        }

        if (input.Color != null)
        {
            garment.Color = input.Color;
        }

        if (input.Pattern != null)
        {
            garment.Pattern = input.Pattern;
        }

        if (input.Seasons != null)
        {
            garment.Seasons = input.Seasons;
        }

        if (input.Formality.HasValue)
        {
            garment.Formality = input.Formality.Value;
        }

        if (input.Size != null)
        {
            garment.Size = input.Size;
        }

        if (input.Visibility.HasValue)
        {
            garment.Visibility = input.Visibility.Value;
        }

        if (input.Notes != null)
        {
            garment.Notes = input.Notes;
        }

        await _garmentRepository.UpdateAsync(garment, autoSave: true);
        return ObjectMapper.Map<Garment, GarmentDto>(garment);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var garment = await _garmentRepository.GetAsync(id);
        EnsureOwner(garment);
        await _garmentRepository.DeleteAsync(garment, autoSave: true);
    }

    public virtual async Task<GarmentDto> CreateFromUrlAsync(CreateGarmentFromUrlDto input)
    {
        var imageUrl = await _aiPipeline.ExtractProductImageFromUrlAsync(input.PageUrl);
        if (string.IsNullOrEmpty(imageUrl))
        {
            throw new UserFriendlyException("Bu site desteklenmiyor. Screenshot yükleyin.");
        }

        return await CreateAsync(new CreateGarmentDto
        {
            OriginalImageUrl = imageUrl,
            Source = GarmentSource.Url,
            Visibility = input.Visibility,
            Size = input.Size,
            Notes = input.Notes
        });
    }

    public virtual Task<GarmentDto> CreateFromPhotoAsync(CreateGarmentFromPhotoDto input)
    {
        return CreateAsync(new CreateGarmentDto
        {
            OriginalImageUrl = input.ImageUrl,
            Source = GarmentSource.Photo,
            Visibility = input.Visibility,
            Size = input.Size,
            Notes = input.Notes
        });
    }

    public virtual async Task<GarmentAnalyzeResultDto> AnalyzeImageAsync(GarmentAnalyzeInputDto input)
    {
        var isClean = await _aiPipeline.CheckNsfwAsync(input.ImageUrl);
        if (!isClean)
        {
            throw new UserFriendlyException("Bu görsel uygun bulunmadı.");
        }

        var r = await _aiPipeline.ProcessGarmentImageAsync(input.ImageUrl);
        return new GarmentAnalyzeResultDto
        {
            Category = r.Category,
            SubCategory = r.SubCategory,
            Color = r.Color,
            Pattern = r.Pattern,
            Seasons = r.Seasons,
            Formality = r.Formality
        };
    }

    public virtual async Task<WardrobeInsightsDto> GetInsightsAsync()
    {
        var userId = CurrentUser.GetId();
        var q = await _garmentRepository.GetQueryableAsync();
        var mine = q.Where(g => g.UserId == userId);
        var total = await _asyncExecuter.CountAsync(mine);
        if (total == 0)
        {
            return new WardrobeInsightsDto
            {
                Summary = "Dolabın henüz boş. Önce parça ekle.",
                DarkGarmentRatio = 0
            };
        }

        var dark = await _asyncExecuter.CountAsync(mine.Where(g =>
            g.Color.Contains("siyah", StringComparison.OrdinalIgnoreCase) ||
            g.Color.Contains("black", StringComparison.OrdinalIgnoreCase)));

        var ratio = (double)dark / total;
        return new WardrobeInsightsDto
        {
            Summary =
                $"Dolabının yaklaşık %{Math.Round(ratio * 100, 0).ToString(CultureInfo.InvariantCulture)}'i koyu tonlarda. Camel ve bej parçalar kombin çeşitliliğini artırır.",
            DarkGarmentRatio = Math.Round(ratio, 2)
        };
    }

    private static IQueryable<Garment> ApplySorting(IQueryable<Garment> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(g => g.CreationTime);
        }

        return sorting.Trim().ToLowerInvariant() switch
        {
            "category" => query.OrderBy(g => g.Category),
            "subcategory" => query.OrderBy(g => g.SubCategory),
            "color" => query.OrderBy(g => g.Color),
            "creationtime asc" => query.OrderBy(g => g.CreationTime),
            "creationtime desc" => query.OrderByDescending(g => g.CreationTime),
            _ => query.OrderByDescending(g => g.CreationTime)
        };
    }

    private void EnsureOwner(Garment garment)
    {
        if (garment.UserId != CurrentUser.GetId())
        {
            throw new BusinessException(OVODomainErrorCodes.GarmentAccessDenied)
                .WithData("GarmentId", garment.Id);
        }
    }
}
