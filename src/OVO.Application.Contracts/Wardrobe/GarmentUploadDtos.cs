using System;

namespace OVO.Wardrobe;

/// <summary>İstemcinin presigned PUT alması için (dosya uzantısı).</summary>
public class GarmentUploadPresignInputDto
{
    /// <summary>Örn. ".jpg", ".png" (noktalı).</summary>
    public string FileExtension { get; set; } = ".jpg";

    /// <summary>60–604800 sn.</summary>
    public int ExpirySeconds { get; set; } = 3600;
}

public class GarmentUploadPresignResultDto
{
    public Guid GarmentId { get; set; }

    public string Bucket { get; set; } = "";

    public string ObjectKey { get; set; } = "";

    public string UploadUrl { get; set; } = "";

    public DateTime ExpiresAtUtc { get; set; }
}

/// <summary>Presigned PUT sonrası kıyafet kaydı (ObjectKey, kullanıcı ve GarmentId eşleşmeli).</summary>
public class CreateGarmentAfterClientUploadDto
{
    public Guid GarmentId { get; set; }

    public string ObjectKey { get; set; } = "";

    public string? BucketName { get; set; }

    public GarmentSource Source { get; set; } = GarmentSource.Upload;

    public GarmentCategory? Category { get; set; }

    public string? SubCategory { get; set; }

    public string? Color { get; set; }

    public string? Pattern { get; set; }

    public string? Seasons { get; set; }

    public GarmentFormality? Formality { get; set; }

    public string? Size { get; set; }

    public GarmentVisibility Visibility { get; set; } = GarmentVisibility.Visible;

    public string? Notes { get; set; }
}

/// <summary>Multipart metadata (form alanı: metadataJson).</summary>
public class CreateGarmentMultipartMetadataDto
{
    public GarmentSource Source { get; set; } = GarmentSource.Upload;

    public GarmentCategory? Category { get; set; }

    public string? SubCategory { get; set; }

    public string? Color { get; set; }

    public string? Pattern { get; set; }

    public string? Seasons { get; set; }

    public GarmentFormality? Formality { get; set; }

    public string? Size { get; set; }

    public GarmentVisibility Visibility { get; set; } = GarmentVisibility.Visible;

    public string? Notes { get; set; }
}
