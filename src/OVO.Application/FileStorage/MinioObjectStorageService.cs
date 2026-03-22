using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Volo.Abp;

namespace OVO.FileStorage;

public sealed class MinioObjectStorageService : IObjectStorageService, IDisposable
{
    private readonly MinioStorageOptions _options;
    private readonly IMinioClient _client;
    private bool _disposed;

    public MinioObjectStorageService(IOptions<MinioStorageOptions> optionsAccessor)
    {
        _options = optionsAccessor.Value;
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new BusinessException(OVODomainErrorCodes.ObjectStorageNotConfigured)
                .WithData("Detail", "Minio:Endpoint boş.");
        }

        if (string.IsNullOrWhiteSpace(_options.AccessKey) || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new BusinessException(OVODomainErrorCodes.ObjectStorageNotConfigured)
                .WithData("Detail", "Minio:AccessKey / SecretKey gerekli.");
        }

        IMinioClient client = new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSsl);

        if (!string.IsNullOrWhiteSpace(_options.Region))
        {
            client = client.WithRegion(_options.Region!);
        }

        _client = client.Build();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_client is IDisposable d)
        {
            d.Dispose();
        }
    }

    public async Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
            var exists = await _client.BucketExistsAsync(existsArgs, cancellationToken).ConfigureAwait(false);
            if (exists)
            {
                return;
            }

            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            if (!string.IsNullOrWhiteSpace(_options.Region))
            {
                makeArgs.WithLocation(_options.Region!);
            }

            await _client.MakeBucketAsync(makeArgs, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException or BucketNotFoundException)
        {
            throw Wrap(ex, "Bucket oluşturulamadı veya erişilemedi.");
        }
    }

    public async Task<ObjectPutResult> PutObjectAsync(ObjectPutRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var size = ResolveObjectSize(request.Content, request.ObjectSize);

        try
        {
            await EnsureBucketAsync(request.BucketName, cancellationToken).ConfigureAwait(false);

            var putArgs = new PutObjectArgs()
                .WithBucket(request.BucketName)
                .WithObject(request.ObjectKey)
                .WithStreamData(request.Content)
                .WithObjectSize(size)
                .WithContentType(request.ContentType);

            if (request.Metadata is { Count: > 0 })
            {
                putArgs.WithHeaders(request.Metadata.ToDictionary(k => k.Key, k => k.Value, StringComparer.Ordinal));
            }

            var response = await _client.PutObjectAsync(putArgs, cancellationToken).ConfigureAwait(false);
            return new ObjectPutResult
            {
                BucketName = request.BucketName,
                ObjectKey = request.ObjectKey,
                ETag = response.Etag,
                Size = response.Size > 0 ? response.Size : size
            };
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne yüklenemedi.");
        }
    }

    public async Task<ObjectPutResult> PutTextAsync(
        string bucketName,
        string objectKey,
        string content,
        string contentType,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content ?? string.Empty);
        await using var ms = new MemoryStream(bytes, writable: false);
        return await PutObjectAsync(
            new ObjectPutRequest
            {
                BucketName = bucketName,
                ObjectKey = objectKey,
                Content = ms,
                ObjectSize = bytes.Length,
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "text/plain; charset=utf-8" : contentType,
                Metadata = metadata
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey);
            await _client.RemoveObjectAsync(args, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne silinemedi.");
        }
    }

    public async Task<bool> ObjectExistsAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var statArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey);
            _ = await _client.StatObjectAsync(statArgs, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne varlığı kontrol edilemedi.");
        }
    }

    public async Task<ObjectMetadata?> StatObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var statArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey);
            var stat = await _client.StatObjectAsync(statArgs, cancellationToken).ConfigureAwait(false);
            return MapStat(stat);
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne bilgisi alınamadı.");
        }
    }

    public async Task<byte[]> GetObjectBytesAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
    {
        await using var ms = new MemoryStream();
        await GetObjectStreamAsync(bucketName, objectKey, ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }

    public async Task GetObjectStreamAsync(
        string bucketName,
        string objectKey,
        Stream destination,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var getArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithCallbackStream(async (stream, ct) =>
                {
                    await stream.CopyToAsync(destination, ct).ConfigureAwait(false);
                });

            await _client.GetObjectAsync(getArgs, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne indirilemedi.");
        }
    }

    public Task<string> PresignedPutObjectAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var seconds = (int)Math.Clamp(expiry.TotalSeconds, 1, 60 * 60 * 24 * 7);
            var args = new PresignedPutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithExpiry(seconds);
            return _client.PresignedPutObjectAsync(args);
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Presigned PUT URL üretilemedi.");
        }
    }

    public Task<string> PresignedGetObjectAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var seconds = (int)Math.Clamp(expiry.TotalSeconds, 1, 60 * 60 * 24 * 7);
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithExpiry(seconds);
            return _client.PresignedGetObjectAsync(args);
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Presigned GET URL üretilemedi.");
        }
    }

    public async Task CopyObjectAsync(
        string sourceBucket,
        string sourceKey,
        string destinationBucket,
        string destinationKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statArgs = new StatObjectArgs()
                .WithBucket(sourceBucket)
                .WithObject(sourceKey);
            var stat = await _client.StatObjectAsync(statArgs, cancellationToken).ConfigureAwait(false);

            var tempPath = Path.Combine(Path.GetTempPath(), $"ovo-minio-{Guid.NewGuid():N}.bin");
            try
            {
                await using (var fs = new FileStream(
                                 tempPath,
                                 FileMode.CreateNew,
                                 FileAccess.Write,
                                 FileShare.None,
                                 bufferSize: 81920,
                                 FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await GetObjectStreamAsync(sourceBucket, sourceKey, fs, cancellationToken).ConfigureAwait(false);
                }

                await using var readFs = new FileStream(
                    tempPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 81920,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);
                var len = readFs.Length;
                await PutObjectAsync(
                        new ObjectPutRequest
                        {
                            BucketName = destinationBucket,
                            ObjectKey = destinationKey,
                            Content = readFs,
                            ObjectSize = len,
                            ContentType = string.IsNullOrWhiteSpace(stat.ContentType)
                                ? "application/octet-stream"
                                : stat.ContentType
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        /* temp temizliği başarısız olsa da akış devam eder */
                    }
                }
            }
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne kopyalanamadı.");
        }
    }

    public async Task<IReadOnlyList<StoredObjectItem>> ListObjectsAsync(
        string bucketName,
        string? prefix,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var listArgs = new ListObjectsArgs()
                .WithBucket(bucketName)
                .WithPrefix(prefix ?? "")
                .WithRecursive(recursive);

            var list = new List<StoredObjectItem>();
            await foreach (var item in _client.ListObjectsEnumAsync(listArgs, cancellationToken).ConfigureAwait(false))
            {
                if (item.IsDir)
                {
                    continue;
                }

                list.Add(
                    new StoredObjectItem
                    {
                        ObjectKey = item.Key,
                        Size = (long)item.Size,
                        ETag = item.ETag,
                        LastModifiedUtc = item.LastModifiedDateTime is { } lm ? NormalizeUtc(lm) : null
                    });
            }

            return list;
        }
        catch (Exception ex) when (ex is MinioException or ConnectionException)
        {
            throw Wrap(ex, "Nesne listelenemedi.");
        }
    }

    private static long ResolveObjectSize(Stream content, long declaredSize)
    {
        if (declaredSize > 0)
        {
            return declaredSize;
        }

        if (content.CanSeek)
        {
            return content.Length;
        }

        throw new UserFriendlyException(
            "Akış boyutu bilinmiyor. ObjectSize belirtin veya CanSeek=true bir akış kullanın.");
    }

    private static ObjectMetadata MapStat(ObjectStat stat)
    {
        var meta = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in stat.MetaData)
        {
            meta[kv.Key] = kv.Value;
        }

        return new ObjectMetadata
        {
            Size = stat.Size,
            ContentType = stat.ContentType,
            ETag = stat.ETag,
            LastModifiedUtc = NormalizeUtc(stat.LastModified),
            UserMetadata = meta
        };
    }

    private static DateTime? NormalizeUtc(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
        };
    }

    private static BusinessException Wrap(Exception inner, string message)
    {
        return new BusinessException(
                OVODomainErrorCodes.ObjectStorageOperationFailed,
                $"{message} ({inner.Message})")
            .WithData("InnerType", inner.GetType().Name);
    }
}
