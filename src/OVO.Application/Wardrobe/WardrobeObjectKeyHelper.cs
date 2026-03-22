using System;
using System.IO;

namespace OVO.Wardrobe;

internal static class WardrobeObjectKeyHelper
{
    public static string BuildOriginalKey(Guid userId, Guid garmentId, string extensionWithDot)
    {
        var ext = NormalizeExtension(extensionWithDot);
        return $"users/{userId:N}/garments/{garmentId:N}/original{ext}";
    }

    public static bool TryParseOriginalKey(string objectKey, Guid currentUserId, out Guid garmentId)
    {
        garmentId = default;
        var prefix = $"users/{currentUserId:N}/garments/";
        if (!objectKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rest = objectKey.AsSpan(prefix.Length);
        var sep = rest.IndexOf('/');
        if (sep <= 0)
        {
            return false;
        }

        if (!Guid.TryParseExact(rest.Slice(0, sep), "N", out garmentId))
        {
            return false;
        }

        var filePart = rest.Slice(sep + 1);
        if (!filePart.StartsWith("original.", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public static string NormalizeExtension(string? extensionOrFileName)
    {
        if (string.IsNullOrWhiteSpace(extensionOrFileName))
        {
            return ".jpg";
        }

        var s = extensionOrFileName.Trim();
        if (!s.StartsWith('.'))
        {
            s = Path.GetExtension(s);
        }

        if (string.IsNullOrEmpty(s))
        {
            return ".jpg";
        }

        return s.ToLowerInvariant();
    }

    public static bool IsAllowedImageExtension(string extensionWithDot)
    {
        var e = NormalizeExtension(extensionWithDot);
        return e is ".jpg" or ".jpeg" or ".png" or ".webp";
    }
}
