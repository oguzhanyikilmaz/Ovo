using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OVO.FileStorage;

/// <summary>
/// S3 uyumlu nesne depolama soyutlaması (MinIO veya AWS S3).
/// Görseller, PDF, sözleşme onay snapshot'ları (JSON) vb. için ortak kullanım.
/// </summary>
public interface IObjectStorageService
{
    /// <summary>
    /// Bucket yoksa oluşturur (idempotent).
    /// </summary>
    Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nesneyi yükler; kullanıcı metadata'sı S3 x-amz-meta-* olarak yazılır.
    /// </summary>
    Task<ObjectPutResult> PutObjectAsync(ObjectPutRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// UTF-8 metin veya JSON snapshot (ör. sözleşme onayı) yükler.
    /// </summary>
    Task<ObjectPutResult> PutTextAsync(
        string bucketName,
        string objectKey,
        string content,
        string contentType,
        IReadOnlyDictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    Task RemoveObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);

    Task<bool> ObjectExistsAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);

    Task<ObjectMetadata?> StatObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nesneyi bellekte döndürür; büyük dosyalar için <see cref="GetObjectStreamAsync"/> tercih edin.
    /// </summary>
    Task<byte[]> GetObjectBytesAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Nesneyi verilen akışa yazar.
    /// </summary>
    Task GetObjectStreamAsync(
        string bucketName,
        string objectKey,
        Stream destination,
        CancellationToken cancellationToken = default);

    Task<string> PresignedPutObjectAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task<string> PresignedGetObjectAsync(
        string bucketName,
        string objectKey,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sunucu tarafı kopya (aynı veya farklı bucket).
    /// </summary>
    Task CopyObjectAsync(
        string sourceBucket,
        string sourceKey,
        string destinationBucket,
        string destinationKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prefix altındaki nesneleri listeler (sayfalama yok; büyük bucket'larda dikkat).
    /// </summary>
    Task<IReadOnlyList<StoredObjectItem>> ListObjectsAsync(
        string bucketName,
        string? prefix,
        bool recursive,
        CancellationToken cancellationToken = default);
}
