namespace OVO.AiPipeline;

/// <summary>
/// Sanal deneme (try-on) render için yönetilebilir prompt ve çıktı ayarları (<c>Gemini:TryOn</c>).
/// İlk referans görsel stüdyo fotoğrafı, sonrakiler kıyafet kesitleri.
/// </summary>
public sealed class GeminiTryOnOptions
{
    public string AspectRatio { get; set; } = "9:16";

    public string? ImageSize { get; set; } = "2K";

    /// <summary>Doluysa doğrudan modele gider; görsel sırası için <see cref="TryOnStructuredBrief.ImageOrderHint"/> veya metinde belirtin.</summary>
    public string? Prompt { get; set; }

    /// <summary><see cref="Prompt"/> boşken JSON brief olarak eklenir.</summary>
    public TryOnStructuredBrief? StructuredBrief { get; set; }
}

/// <summary>Try-on sahnesi — appsettings’te JSON nesnesi.</summary>
public sealed class TryOnStructuredBrief
{
    public string? Summary { get; set; }

    /// <summary>Örn. ilk görsel = kişi (stüdyo), sonrakiler = kıyafet PNG’leri.</summary>
    public string? ImageOrderHint { get; set; }

    public string? GarmentApplication { get; set; }

    public string? FitAndRealism { get; set; }

    public string? Lighting { get; set; }

    public string? Background { get; set; }

    public string? CameraAndFraming { get; set; }

    public string? OutputFormat { get; set; }

    public string? MustAvoid { get; set; }
}
