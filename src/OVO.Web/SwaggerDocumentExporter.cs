using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace OVO.Web;

internal static class SwaggerDocumentExporter
{
    /// <summary>
    /// Uygulama başlatıldıktan sonra (InitializeApplicationAsync) OpenAPI 3 şemasını dosyaya yazar.
    /// </summary>
    public static Task ExportAsync(WebApplication app, string outputPath, string documentName = "v1")
    {
        var fullPath = SwaggerCli.ResolveOutputPath(outputPath, app.Environment.ContentRootPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var scope = app.Services.CreateScope();
        var swaggerProvider = scope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
        var document = swaggerProvider.GetSwagger(documentName);

        using var writer = new StreamWriter(fullPath);
        var jsonWriter = new OpenApiJsonWriter(writer);
        document.SerializeAsV3(jsonWriter);
        writer.Flush();

        Log.Information("OpenAPI (Swagger) yazıldı: {Path}", fullPath);
        return Task.CompletedTask;
    }
}
