using Volo.Abp.Modularity;

namespace OVO;

/* Inherit from this class for your domain layer tests. */
public abstract class OVODomainTestBase<TStartupModule> : OVOTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
