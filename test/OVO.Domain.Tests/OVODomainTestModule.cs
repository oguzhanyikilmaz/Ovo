using Volo.Abp.Modularity;

namespace OVO;

[DependsOn(
    typeof(OVODomainModule),
    typeof(OVOTestBaseModule)
)]
public class OVODomainTestModule : AbpModule
{

}
