using System;

namespace OVO.FileStorage;

public class GetPresignedPutUrlInput
{
    /// <summary>Boşsa uygulama varsayılan bucket'ı (Minio:DefaultBucket).</summary>
    public string? BucketName { get; set; }

    public string ObjectKey { get; set; } = "";

    /// <summary>60 saniye — 7 gün arası.</summary>
    public int ExpirySeconds { get; set; } = 3600;
}

public class GetPresignedGetUrlInput
{
    public string? BucketName { get; set; }

    public string ObjectKey { get; set; } = "";

    public int ExpirySeconds { get; set; } = 3600;
}

public class PresignedUrlResultDto
{
    public string Url { get; set; } = "";

    public DateTime ExpiresAtUtc { get; set; }
}
