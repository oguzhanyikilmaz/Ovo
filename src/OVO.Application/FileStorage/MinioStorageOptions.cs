namespace OVO.FileStorage;

/// <summary>
/// MinIO / S3 uyumlu depolama. Üretimde CORS ve public URL stratejisi için
/// repodaki <c>infra/minio/uretim-kilavuzu.txt</c> dosyasına bakın.
/// </summary>
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
    /// <para><b>Strateji A — Genel okuma URL’si (CDN veya public bucket):</b>
    /// Dolu bırakın. Wardrobe (ve ileride diğer modüller) AI / harici HTTP için
    /// <c>{PublicReadBaseUrl}/{objectKey}</c> kullanır. Köke bucket yolunu dahil edin
    /// (örn. <c>https://cdn.sizin.com/ovo-media</c>); <c>objectKey</c> bucket adı içermemelidir.</para>
    /// <para><b>Strateji B — Tamamen private bucket:</b> Boş bırakın; uygulama geçici
    /// <b>presigned GET</b> üretir (<see cref="GarmentImagePresignedReadExpirySeconds"/>).</para>
    /// </summary>
    public string? PublicReadBaseUrl { get; set; }

    /// <summary>
    /// <see cref="PublicReadBaseUrl"/> boşken kıyafet görseli için AI pipeline’a verilen
    /// presigned GET süresi (saniye, 60–86400). Kısa = daha dar güvenlik penceresi;
    /// uzun = yavaş AI / retry toleransı.
    /// </summary>
    public int GarmentImagePresignedReadExpirySeconds { get; set; } = 3600;
}
