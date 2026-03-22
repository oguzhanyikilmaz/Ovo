using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OVO.AiPipeline;
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
        context.Services.AddMapperlyObjectMapper<OVOApplicationModule>();
        context.Services.AddTransient<IAiPipelineService, NullAiPipelineService>();
        context.Services.AddValidatorsFromAssemblyContaining<CreateGarmentDtoValidator>();
    }
}
