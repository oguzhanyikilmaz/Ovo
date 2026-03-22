using System;

namespace OVO.FileStorage;

internal static class StorageKeyRules
{
    public static bool IsSafeObjectKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        if (key.StartsWith('/') || key.EndsWith('/') || key.Contains("//", StringComparison.Ordinal))
        {
            return false;
        }

        if (key.Contains("..", StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }
}
