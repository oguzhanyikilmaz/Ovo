using System;
using System.Collections.Generic;
using System.IO;

namespace OVO.Web;

/// <summary>
/// <c>dotnet run --project src/OVO.Web -- --generate-swagger [çıktı yolu]</c> için argüman ayrıştırma.
/// </summary>
internal static class SwaggerCli
{
    internal const string Flag = "--generate-swagger";

    internal const string DefaultRelativeOutputPath = "docs/openapi/swagger.web.v1.full.json";

    /// <summary>
    /// Ana bilgisayar için kullanılacak argümanlar ve isteğe bağlı swagger çıktı dosyası yolu (göreli veya mutlak).
    /// </summary>
    public static (string[] HostArgs, string? SwaggerOutputPath) SplitHostAndSwaggerArgs(string[] args)
    {
        var host = new List<string>(args);
        string? outputPath = null;

        for (var i = 0; i < host.Count; i++)
        {
            var a = host[i];
            if (a == Flag)
            {
                host.RemoveAt(i);
                if (i < host.Count && !host[i].StartsWith('-'))
                {
                    outputPath = host[i];
                    host.RemoveAt(i);
                }
                else
                {
                    outputPath = DefaultRelativeOutputPath;
                }

                break;
            }

            if (a.StartsWith(Flag + "=", StringComparison.Ordinal))
            {
                outputPath = a[(Flag.Length + 1)..];
                host.RemoveAt(i);
                break;
            }
        }

        return (host.ToArray(), outputPath);
    }

    /// <summary>
    /// Göreli yollar: önce depo kökü (üst dizinlerde <c>src/OVO.Web/OVO.Web.csproj</c> aranır), bulunamazsa <see cref="Directory.GetCurrentDirectory"/>.
    /// Mutlak yollar olduğu gibi normalize edilir.
    /// </summary>
    public static string ResolveOutputPath(string outputPath, string contentRootPath)
    {
        if (Path.IsPathRooted(outputPath))
        {
            return Path.GetFullPath(outputPath);
        }

        var repoRoot = TryFindRepositoryRoot(contentRootPath);
        var baseDir = repoRoot ?? Directory.GetCurrentDirectory();
        return Path.GetFullPath(Path.Combine(baseDir, outputPath));
    }

    private static string? TryFindRepositoryRoot(string startDirectory)
    {
        try
        {
            var dir = new DirectoryInfo(Path.GetFullPath(startDirectory));
            for (var i = 0; i < 10 && dir != null; i++)
            {
                var marker = Path.Combine(dir.FullName, "src", "OVO.Web", "OVO.Web.csproj");
                if (File.Exists(marker))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }
        }
        catch (IOException)
        {
            // yol çözülemezse aşağıda CurrentDirectory kullanılır
        }
        catch (UnauthorizedAccessException)
        {
        }

        return null;
    }
}
