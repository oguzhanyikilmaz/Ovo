using System.Collections.Concurrent;
using System.Collections.Generic;
using OVO.Similar;

namespace OVO.Similar;

internal static class SimilarSearchStore
{
    private static readonly ConcurrentDictionary<string, List<SimilarProductDto>> Jobs = new();

    public static string CreateJob(List<SimilarProductDto> results)
    {
        var id = System.Guid.NewGuid().ToString("N");
        Jobs[id] = results;
        return id;
    }

    public static List<SimilarProductDto> GetResults(string jobId)
    {
        return Jobs.TryGetValue(jobId, out var list) ? list : new List<SimilarProductDto>();
    }
}
