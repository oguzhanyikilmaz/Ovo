using System;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OVO.AiPipeline;
using OVO.FileStorage;
using OVO.Wardrobe;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FluentValidation;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace OVO;

[DependsOn(
    typeof(OVODomainModule),
    typeof(AbpAccountApplicationModule),
    typeof(OVOApplicationContractsModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpFluentValidationModule)
    )]
public class OVOApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MinioStorageOptions>(
            configuration.GetSection(MinioStorageOptions.SectionName));

        var minio = configuration.GetSection(MinioStorageOptions.SectionName).Get<MinioStorageOptions>()
                    ?? new MinioStorageOptions();
        var useMinio = minio.Enabled
                       && !string.IsNullOrWhiteSpace(minio.Endpoint)
                       && !string.IsNullOrWhiteSpace(minio.AccessKey)
                       && !string.IsNullOrWhiteSpace(minio.SecretKey);

        if (useMinio)
        {
            context.Services.AddSingleton<IObjectStorageService, MinioObjectStorageService>();
        }
        else
        {
            context.Services.AddSingleton<IObjectStorageService, DisabledObjectStorageService>();
        }

        context.Services.AddMapperlyObjectMapper<OVOApplicationModule>();

        context.Services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        var gemini = configuration.GetSection(GeminiOptions.SectionName).Get<GeminiOptions>() ?? new GeminiOptions();
        if (!string.IsNullOrWhiteSpace(gemini.ApiKey))
        {
            context.Services.AddHttpClient(GeminiAiPipelineService.HttpClientName)
                .ConfigureHttpClient((sp, client) =>
                {
                    var o = sp.GetRequiredService<IOptions<GeminiOptions>>().Value;
                    client.Timeout = TimeSpan.FromSeconds(Math.Max(30, o.HttpClientTimeoutSeconds));
                });
            context.Services.AddTransient<IAiPipelineService, GeminiAiPipelineService>();
        }
        else
        {
            context.Services.AddTransient<IAiPipelineService, NullAiPipelineService>();
        }

        context.Services.AddValidatorsFromAssemblyContaining<CreateGarmentDtoValidator>();
    }
}
