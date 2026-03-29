using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace OVO.Web;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        try
        {
            var (hostArgs, swaggerOut) = SwaggerCli.SplitHostAndSwaggerArgs(args);
            if (swaggerOut is not null)
            {
                Log.Information("OpenAPI dışa aktarımı: {Path}", swaggerOut);
            }
            else
            {
                Log.Information("Starting web host.");
            }

            var builder = WebApplication.CreateBuilder(hostArgs);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            await builder.AddApplicationAsync<OVOWebModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();

            if (swaggerOut is not null)
            {
                await SwaggerDocumentExporter.ExportAsync(app, swaggerOut);
                await app.DisposeAsync();
                return 0;
            }

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            if (ex is HostAbortedException)
            {
                throw;
            }

            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
