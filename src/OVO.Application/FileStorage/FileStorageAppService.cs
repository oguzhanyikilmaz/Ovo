using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OVO.Permissions;
using Volo.Abp;

namespace OVO.FileStorage;

[Authorize(OVOPermissions.FileStorage.Default)]
public class FileStorageAppService : OVOAppService, IFileStorageAppService
{
    private readonly IObjectStorageService _objectStorage;
    private readonly MinioStorageOptions _options;

    public FileStorageAppService(
        IObjectStorageService objectStorage,
        IOptions<MinioStorageOptions> optionsAccessor)
    {
        _objectStorage = objectStorage;
        _options = optionsAccessor.Value;
    }

    public virtual async Task<PresignedUrlResultDto> GetPresignedPutUrlAsync(GetPresignedPutUrlInput input)
    {
        var bucket = ResolveBucket(input.BucketName);
        var expiry = TimeSpan.FromSeconds(input.ExpirySeconds);
        var url = await _objectStorage.PresignedPutObjectAsync(bucket, input.ObjectKey, expiry);
        return new PresignedUrlResultDto
        {
            Url = url,
            ExpiresAtUtc = DateTime.UtcNow.Add(expiry)
        };
    }

    public virtual async Task<PresignedUrlResultDto> GetPresignedGetUrlAsync(GetPresignedGetUrlInput input)
    {
        var bucket = ResolveBucket(input.BucketName);
        var expiry = TimeSpan.FromSeconds(input.ExpirySeconds);
        var url = await _objectStorage.PresignedGetObjectAsync(bucket, input.ObjectKey, expiry);
        return new PresignedUrlResultDto
        {
            Url = url,
            ExpiresAtUtc = DateTime.UtcNow.Add(expiry)
        };
    }

    private string ResolveBucket(string? bucketName)
    {
        if (!string.IsNullOrWhiteSpace(bucketName))
        {
            return bucketName.Trim();
        }

        if (string.IsNullOrWhiteSpace(_options.DefaultBucket))
        {
            throw new BusinessException(OVODomainErrorCodes.ObjectStorageNotConfigured)
                .WithData("Detail", "DefaultBucket yapılandırılmamış.");
        }

        return _options.DefaultBucket;
    }
}
