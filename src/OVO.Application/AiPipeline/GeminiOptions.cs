namespace OVO.AiPipeline;

/// <summary>
/// Google Gemini API (Nano Banana Pro = <see cref="ImageModel"/> varsayılanı).
/// Anahtar: kullanıcı gizleri veya ortam değişkeni; koda gömülmez.
/// </summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    /// <summary>Boş veya yoksa <see cref="NullAiPipelineService"/> kullanılır.</summary>
    public string? ApiKey { get; set; }

    /// <summary>generateContent çağrıları için taban adres.</summary>
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";

    /// <summary>Metin/JSON görüntü anlama (NSFW, etiketleme, foto analizi).</summary>
    public string VisionModel { get; set; } = "gemini-2.0-flash";

    /// <summary>Nano Banana Pro — görüntü üretimi ve düzenleme.</summary>
    public string ImageModel { get; set; } = "gemini-3-pro-image-preview";

    public int VisionTimeoutSeconds { get; set; } = 15;

    public int ImageGenerationTimeoutSeconds { get; set; } = 90;

    /// <summary>HttpClient genel zaman aşımı (görüntü üretiminden büyük olmalı).</summary>
    public int HttpClientTimeoutSeconds { get; set; } = 120;

    /// <summary>Stüdyo fotoğrafı üretim prompt’u ve görüntü parametreleri.</summary>
    public GeminiStudioPhotoOptions StudioPhoto { get; set; } = new();

    /// <summary>Try-on render prompt’u ve görüntü parametreleri.</summary>
    public GeminiTryOnOptions TryOn { get; set; } = new();
}
