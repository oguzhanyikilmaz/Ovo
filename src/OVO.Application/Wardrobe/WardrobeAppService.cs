using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OVO;
using OVO.AiPipeline;
using OVO.FileStorage;
using OVO.Permissions;
using OVO.Wardrobe;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
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
    private readonly IObjectStorageService _objectStorage;
    private readonly MinioStorageOptions _minioOptions;
    private readonly IValidator<CreateGarmentMultipartMetadataDto> _multipartMetadataValidator;

    public WardrobeAppService(
        IGarmentRepository garmentRepository,
        IAiPipelineService aiPipeline,
        IAsyncQueryableExecuter asyncExecuter,
        IObjectStorageService objectStorage,
        IOptions<MinioStorageOptions> minioOptions,
        IValidator<CreateGarmentMultipartMetadataDto> multipartMetadataValidator)
    {
        _garmentRepository = garmentRepository;
        _aiPipeline = aiPipeline;
        _asyncExecuter = asyncExecuter;
        _objectStorage = objectStorage;
        _minioOptions = minioOptions.Value;
        _multipartMetadataValidator = multipartMetadataValidator;
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
        return await CreateGarmentFromOriginalUrlAsync(GuidGenerator.Create(), input);
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

        return await CreateGarmentFromOriginalUrlAsync(
            GuidGenerator.Create(),
            new CreateGarmentDto
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
        return CreateGarmentFromOriginalUrlAsync(
            GuidGenerator.Create(),
            new CreateGarmentDto
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

    public virtual async Task<GarmentUploadPresignResultDto> GetGarmentImageUploadPresignAsync(
        GarmentUploadPresignInputDto input)
    {
        var userId = CurrentUser.GetId();
        var garmentId = GuidGenerator.Create();
        var ext = WardrobeObjectKeyHelper.NormalizeExtension(input.FileExtension);
        var bucket = string.IsNullOrWhiteSpace(_minioOptions.DefaultBucket)
            ? OvoStorageBuckets.Media
            : _minioOptions.DefaultBucket;
        var objectKey = WardrobeObjectKeyHelper.BuildOriginalKey(userId, garmentId, ext);

        await _objectStorage.EnsureBucketAsync(bucket);

        var expiry = TimeSpan.FromSeconds(input.ExpirySeconds);
        var uploadUrl = await _objectStorage.PresignedPutObjectAsync(bucket, objectKey, expiry);

        return new GarmentUploadPresignResultDto
        {
            GarmentId = garmentId,
            Bucket = bucket,
            ObjectKey = objectKey,
            UploadUrl = uploadUrl,
            ExpiresAtUtc = DateTime.UtcNow.Add(expiry)
        };
    }

    public virtual async Task<GarmentDto> CreateAfterGarmentImageUploadAsync(CreateGarmentAfterClientUploadDto input)
    {
        var userId = CurrentUser.GetId();
        if (!WardrobeObjectKeyHelper.TryParseOriginalKey(input.ObjectKey, userId, out var keyGarmentId) ||
            keyGarmentId != input.GarmentId)
        {
            throw new UserFriendlyException("ObjectKey ile GarmentId eşleşmiyor veya bu kullanıcıya ait değil.");
        }

        var bucket = string.IsNullOrWhiteSpace(input.BucketName)
            ? _minioOptions.DefaultBucket
            : input.BucketName!.Trim();

        if (string.IsNullOrWhiteSpace(bucket))
        {
            throw new UserFriendlyException("Bucket adı yapılandırılmamış.");
        }

        if (await _garmentRepository.FindAsync(input.GarmentId) != null)
        {
            throw new UserFriendlyException("Bu kıyafet kaydı zaten oluşturulmuş.");
        }

        if (!await _objectStorage.ObjectExistsAsync(bucket, input.ObjectKey))
        {
            throw new UserFriendlyException("Yükleme bulunamadı. Önce presigned URL ile dosyayı yükleyin.");
        }

        var imageUrl = await ResolveImageUrlForPipelineAsync(bucket, input.ObjectKey, default);

        var createDto = new CreateGarmentDto
        {
            OriginalImageUrl = imageUrl,
            Source = input.Source,
            Category = input.Category,
            SubCategory = input.SubCategory,
            Color = input.Color,
            Pattern = input.Pattern,
            Seasons = input.Seasons,
            Formality = input.Formality,
            Size = input.Size,
            Visibility = input.Visibility,
            Notes = input.Notes
        };

        return await CreateGarmentFromOriginalUrlAsync(input.GarmentId, createDto);
    }

    [RemoteService(IsEnabled = false)]
    public virtual async Task<GarmentDto> CreateFromUploadStreamAsync(
        Stream fileStream,
        string fileName,
        string? contentType,
        long? contentLength,
        CreateGarmentMultipartMetadataDto metadata,
        CancellationToken cancellationToken = default)
    {
        Check.NotNull(fileStream, nameof(fileStream));
        await _multipartMetadataValidator.ValidateAndThrowAsync(metadata, cancellationToken);

        var ext = WardrobeObjectKeyHelper.NormalizeExtension(Path.GetExtension(fileName));
        if (!WardrobeObjectKeyHelper.IsAllowedImageExtension(ext))
        {
            throw new UserFriendlyException("Desteklenen formatlar: JPG, PNG, WEBP.");
        }

        var userId = CurrentUser.GetId();
        var garmentId = GuidGenerator.Create();
        var bucket = string.IsNullOrWhiteSpace(_minioOptions.DefaultBucket)
            ? OvoStorageBuckets.Media
            : _minioOptions.DefaultBucket;
        var objectKey = WardrobeObjectKeyHelper.BuildOriginalKey(userId, garmentId, ext);

        var size = contentLength ?? (fileStream.CanSeek ? fileStream.Length : 0L);
        if (size <= 0)
        {
            throw new UserFriendlyException("Dosya boyutu okunamadı veya sıfır.");
        }

        if (size > WardrobeUploadLimits.MaxImageBytes)
        {
            throw new UserFriendlyException("Dosya çok büyük (en fazla 20 MB).");
        }

        var resolvedContentType = string.IsNullOrWhiteSpace(contentType)
            ? GuessContentType(ext)
            : contentType!;

        await _objectStorage.PutObjectAsync(
            new ObjectPutRequest
            {
                BucketName = bucket,
                ObjectKey = objectKey,
                Content = fileStream,
                ObjectSize = size,
                ContentType = resolvedContentType
            },
            cancellationToken);

        var imageUrl = await ResolveImageUrlForPipelineAsync(bucket, objectKey, cancellationToken);

        var createDto = new CreateGarmentDto
        {
            OriginalImageUrl = imageUrl,
            Source = metadata.Source,
            Category = metadata.Category,
            SubCategory = metadata.SubCategory,
            Color = metadata.Color,
            Pattern = metadata.Pattern,
            Seasons = metadata.Seasons,
            Formality = metadata.Formality,
            Size = metadata.Size,
            Visibility = metadata.Visibility,
            Notes = metadata.Notes
        };

        return await CreateGarmentFromOriginalUrlAsync(garmentId, createDto);
    }

    private async Task<string> ResolveImageUrlForPipelineAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_minioOptions.PublicReadBaseUrl))
        {
            var root = _minioOptions.PublicReadBaseUrl!.TrimEnd('/');
            var key = objectKey.TrimStart('/');
            return $"{root}/{key}";
        }

        var seconds = Math.Clamp(_minioOptions.GarmentImagePresignedReadExpirySeconds, 60, 86400);
        return await _objectStorage.PresignedGetObjectAsync(
            bucket,
            objectKey,
            TimeSpan.FromSeconds(seconds),
            cancellationToken);
    }

    private static string GuessContentType(string extWithDot)
    {
        return extWithDot.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".jpg" or ".jpeg" => "image/jpeg",
            _ => "application/octet-stream"
        };
    }

    private async Task<GarmentDto> CreateGarmentFromOriginalUrlAsync(Guid garmentId, CreateGarmentDto input)
    {
        var isClean = await _aiPipeline.CheckNsfwAsync(input.OriginalImageUrl);
        if (!isClean)
        {
            throw new UserFriendlyException("Bu görsel uygun bulunmadı.");
        }

        var processed = await _aiPipeline.ProcessGarmentImageAsync(input.OriginalImageUrl);

        var garment = new Garment(
            garmentId,
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
