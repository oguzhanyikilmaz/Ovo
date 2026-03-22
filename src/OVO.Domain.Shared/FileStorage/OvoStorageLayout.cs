using System;
using System.Globalization;

namespace OVO.FileStorage;

/// <summary>
/// Önerilen bucket adları ve nesne anahtarı şablonları (AGENTS.md yolları ile uyumlu).
/// </summary>
public static class OvoStorageBuckets
{
    /// <summary>Görseller, render çıktıları, genel medya.</summary>
    public const string Media = "ovo-media";

    /// <summary>Sözleşme PDF / compliance arşivi (isteğe bağlı ayrı bucket).</summary>
    public const string Legal = "ovo-legal";
}

public static class OvoStoragePaths
{
    public static string UserTryOnPhoto(Guid userId, string fileIdWithExtension) =>
        $"users/{userId:N}/tryon/photos/{fileIdWithExtension}";

    public static string UserStudioPhoto(Guid userId) =>
        $"users/{userId:N}/studio.jpg";

    public static string UserPhoto(Guid userId, string fileIdWithExtension) =>
        $"users/{userId:N}/photos/{fileIdWithExtension}";

    public static string GarmentOriginal(Guid garmentId, string extensionWithDot) =>
        $"garments/{garmentId:N}/original{extensionWithDot}";

    public static string GarmentCutout(Guid garmentId) =>
        $"garments/{garmentId:N}/cutout.png";

    public static string RenderResult(string renderHash) =>
        $"renders/{renderHash}.jpg";

    /// <summary>
    /// Kullanıcı sözleşme onayı snapshot'ı (JSON önerilir). Zaman UTC ile sabitlenir.
    /// </summary>
    public static string UserConsentSnapshot(Guid userId, string agreementCode, DateTimeOffset acceptedAtUtc) =>
        string.Format(
            CultureInfo.InvariantCulture,
            "users/{0:N}/consents/{1}/{2:yyyyMMddTHHmmss}Z.json",
            userId,
            SanitizePathSegment(agreementCode),
            acceptedAtUtc.UtcDateTime);

    /// <summary>Genel hukuki / metin belgeleri.</summary>
    public static string LegalDocument(string documentCode, string version, string fileName) =>
        $"legal/{SanitizePathSegment(documentCode)}/{SanitizePathSegment(version)}/{SanitizePathSegment(fileName)}";

    private static string SanitizePathSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        var trimmed = value.Trim();
        foreach (var c in PathInvalidChars)
        {
            trimmed = trimmed.Replace(c, '-');
        }

        return trimmed;
    }

    private static readonly char[] PathInvalidChars =
    {
        '/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0', '\n', '\r'
    };
}
