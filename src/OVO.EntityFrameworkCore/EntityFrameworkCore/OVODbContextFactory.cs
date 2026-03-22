using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OVO.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class OVODbContextFactory : IDesignTimeDbContextFactory<OVODbContext>
{
    public OVODbContext CreateDbContext(string[] args)
    {
        OVOEfCoreEntityExtensionMappings.Configure();

        var configuration = BuildConfiguration();

        var cs = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Default tanımlı değil. OVO.DbMigrator/appsettings.secrets.json içine ekleyin.");
        }

        var builder = new DbContextOptionsBuilder<OVODbContext>()
            .UseNpgsql(cs);

        return new OVODbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../OVO.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.secrets.json", optional: true);

        return builder.Build();
    }
}
