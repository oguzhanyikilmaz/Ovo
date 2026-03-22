using OVO.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace OVO.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(OVOEntityFrameworkCoreModule),
    typeof(OVOApplicationContractsModule)
    )]
public class OVODbMigratorModule : AbpModule
{
}
