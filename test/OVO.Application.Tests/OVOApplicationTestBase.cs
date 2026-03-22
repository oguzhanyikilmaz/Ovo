using Volo.Abp.Modularity;

namespace OVO;

public abstract class OVOApplicationTestBase<TStartupModule> : OVOTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
