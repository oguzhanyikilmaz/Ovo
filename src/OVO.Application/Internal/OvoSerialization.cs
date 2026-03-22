using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OVO.Community;

namespace OVO.Internal;

internal static class OvoSerialization
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string GarmentIdsToJson(IReadOnlyList<Guid> ids)
    {
        return JsonSerializer.Serialize(ids.OrderBy(x => x).ToArray(), JsonOptions);
    }

    public static List<Guid> GarmentIdsFromJson(string json)
    {
        var list = JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions);
        return list ?? new List<Guid>();
    }

    public static string IntArrayToJson(int[] values)
    {
        return JsonSerializer.Serialize(values, JsonOptions);
    }

    public static int[] IntArrayFromJson(string json)
    {
        var arr = JsonSerializer.Deserialize<int[]>(json, JsonOptions);
        return arr ?? Array.Empty<int>();
    }

    public static string PollOptionsToJson(List<PollOptionDto> options)
    {
        return JsonSerializer.Serialize(options, JsonOptions);
    }

    public static List<PollOptionDto> PollOptionsFromJson(string json)
    {
        return JsonSerializer.Deserialize<List<PollOptionDto>>(json, JsonOptions) ?? new List<PollOptionDto>();
    }
}
