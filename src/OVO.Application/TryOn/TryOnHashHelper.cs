using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OVO.TryOn;

internal static class TryOnHashHelper
{
    public static string BuildComboHash(Guid userId, IReadOnlyList<Guid> garmentIds)
    {
        var sorted = string.Join(",", garmentIds.OrderBy(x => x));
        var raw = $"{userId:N}:{sorted}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
