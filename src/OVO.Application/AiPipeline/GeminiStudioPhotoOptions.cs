namespace OVO.AiPipeline;

/// <summary>
/// Stüdyo fotoğrafı üretimi için yönetilebilir prompt ve çıktı ayarları (<c>Gemini:StudioPhoto</c>).
/// <see cref="Prompt"/> doluysa doğrudan modele gider; boşsa <see cref="StructuredBrief"/> JSON olarak eklenir.
/// </summary>
public sealed class GeminiStudioPhotoOptions
{
    /// <summary>Çıktı en-boy oranı (örn. 9:16, 3:4).</summary>
    public string AspectRatio { get; set; } = "9:16";

    /// <summary>Çözünürlük (Gemini 3: 1K, 2K, 4K).</summary>
    public string? ImageSize { get; set; } = "2K";

    /// <summary>
    /// Modele gidecek tam talimat metni. Doluysa <see cref="StructuredBrief"/> kullanılmaz.
    /// İçinde kendi JSON şablonunuzu, markdown veya düz metin yazabilirsiniz.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// <see cref="Prompt"/> boşken detaylı sahne/ışık/poz vb. için yapılandırılmış brief;
    /// JSON olarak modele eklenir.
    /// </summary>
    public StudioPhotoStructuredBrief? StructuredBrief { get; set; }
}

/// <summary>
/// Stüdyo sahnesi için alanlar — appsettings’te JSON nesnesi olarak doldurulur.
/// </summary>
public sealed class StudioPhotoStructuredBrief
{
    public string? Summary { get; set; }

    public string? Background { get; set; }

    public string? Lighting { get; set; }

    public string? CameraAndFraming { get; set; }

    public string? PoseAndExpression { get; set; }

    public string? SkinAndRealism { get; set; }

    public string? OutputFormat { get; set; }

    public string? MustAvoid { get; set; }
}
