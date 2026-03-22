using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace OVO.FileStorage;

/// <summary>
/// MinIO kapalıyken DI'nın kırılmaması için; tüm çağrılar anlamlı hata fırlatır.
/// </summary>
public sealed class DisabledObjectStorageService : IObjectStorageService
{
    private static void Throw()
    {
        throw new BusinessException(OVODomainErrorCodes.ObjectStorageNotConfigured)
            .WithData("Hint", "Minio:Enabled=true ve Endpoint/AccessKey/SecretKey yapılandırın.");
    }

    public Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.CompletedTask;
    }

    public Task<ObjectPutResult> PutObjectAsync(ObjectPutRequest request, CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.FromResult<ObjectPutResult>(null!);
    }

    public Task<ObjectPutResult> PutTextAsync(
        string bucketName,
        string objectKey,
        string content,
        string contentType,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.FromResult<ObjectPutResult>(null!);
    }

    public Task RemoveObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.CompletedTask;
    }

    public Task<bool> ObjectExistsAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }

    public Task<ObjectMetadata?> StatObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ObjectMetadata?>(null);
    }

    public Task<byte[]> GetObjectBytesAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.FromResult<byte[]>(null!);
    }

    public Task GetObjectStreamAsync(
        string bucketName,
        string objectKey,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.CompletedTask;
    }

    public Task<string> PresignedPutObjectAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.FromResult<string>(null!);
    }

    public Task<string> PresignedGetObjectAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.FromResult<string>(null!);
    }

    public Task CopyObjectAsync(
        string sourceBucket,
        string sourceKey,
        string destinationBucket,
        string destinationKey,
        CancellationToken cancellationToken = default)
    {
        Throw();
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<StoredObjectItem>> ListObjectsAsync(
        string bucketName,
        string? prefix,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<StoredObjectItem>>(Array.Empty<StoredObjectItem>());
    }
}
