using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace OVO.DbMigrator;

class Program
{
    static async Task Main(string[] args)
    {
        // Npgsql: timestamptz yalnızca UTC kabul eder; ABP Identity seed Local DateTime üretir.
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel.Override("OVO", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("OVO", LogEventLevel.Information)
#endif
                .Enrich.FromLogContext()
            .WriteTo.Async(c => c.File("Logs/logs.txt"))
            .WriteTo.Async(c => c.Console())
            .CreateLogger();

        await CreateHostBuilder(args).RunConsoleAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        // IDE / farklı çalışma dizininde appsettings*.json bulunabilsin diye çıktı klasörünü kök yap.
        var contentRoot = AppContext.BaseDirectory;
        return Host.CreateDefaultBuilder(args)
            .UseContentRoot(contentRoot)
            .ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                configurationBuilder.AddJsonFile(
                    Path.Combine(contentRoot, "appsettings.secrets.json"),
                    optional: true,
                    reloadOnChange: false);
            })
            .AddAppSettingsSecretsJson()
            .ConfigureLogging((context, logging) => logging.ClearProviders())
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<DbMigratorHostedService>();
            });
    }
}
