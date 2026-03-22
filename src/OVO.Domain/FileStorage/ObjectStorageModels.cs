using System;
using System.Collections.Generic;
using System.IO;

namespace OVO.FileStorage;

public sealed class ObjectPutRequest
{
    public required string BucketName { get; init; }

    public required string ObjectKey { get; init; }

    public required Stream Content { get; init; }

    /// <summary>Akış uzunluğu. Sıfırsa ve akış <see cref="Stream.CanSeek"/> ise uzunluk otomatik alınır.</summary>
    public long ObjectSize { get; init; }

    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>Özel başlıklar; anahtarlar x-amz-meta- ile öneklenir (SDK davranışı).</summary>
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}

public sealed class ObjectPutResult
{
    public required string BucketName { get; init; }

    public required string ObjectKey { get; init; }

    public string? ETag { get; init; }

    public long Size { get; init; }
}

public sealed class ObjectMetadata
{
    public required long Size { get; init; }

    public string? ContentType { get; init; }

    public string? ETag { get; init; }

    public DateTime? LastModifiedUtc { get; init; }

    public IReadOnlyDictionary<string, string> UserMetadata { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed class StoredObjectItem
{
    public required string ObjectKey { get; init; }

    public long Size { get; init; }

    public DateTime? LastModifiedUtc { get; init; }

    public string? ETag { get; init; }
}
