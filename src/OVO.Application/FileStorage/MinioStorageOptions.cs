namespace OVO.FileStorage;

public class MinioStorageOptions
{
    public const string SectionName = "Minio";

    /// <summary>
    /// false ise <see cref="DisabledObjectStorageService"/> kaydedilir (yerel geliştirme).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Örn. localhost:9000 veya minio.example.com (şema yok).</summary>
    public string Endpoint { get; set; } = "";

    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public bool UseSsl { get; set; }

    public string? Region { get; set; }

    /// <summary>API'de bucket verilmezse kullanılır.</summary>
    public string DefaultBucket { get; set; } = OvoStorageBuckets.Media;

    /// <summary>
    /// Kıyafet görseli için harici erişim kökü (bucket yolu dahil edilebilir).
    /// Örn. https://cdn.ornek.com/ovo-media — URL = kök + "/" + objectKey.
    /// Boşsa Banana/AI için presigned GET kullanılır.
    /// </summary>
    public string? PublicReadBaseUrl { get; set; }

    /// <summary>Public URL yokken görseli AI servisine vermek için presigned GET süresi (sn).</summary>
    public int GarmentImagePresignedReadExpirySeconds { get; set; } = 3600;
}
