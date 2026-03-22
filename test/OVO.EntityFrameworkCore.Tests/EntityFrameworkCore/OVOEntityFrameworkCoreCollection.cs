using Xunit;

namespace OVO.EntityFrameworkCore;

[CollectionDefinition(OVOTestConsts.CollectionDefinitionName)]
public class OVOEntityFrameworkCoreCollection : ICollectionFixture<OVOEntityFrameworkCoreFixture>
{

}
