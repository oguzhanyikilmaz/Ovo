using Volo.Abp.Modularity;

namespace OVO;

[DependsOn(
    typeof(OVOApplicationModule),
    typeof(OVODomainTestModule)
)]
public class OVOApplicationTestModule : AbpModule
{

}
