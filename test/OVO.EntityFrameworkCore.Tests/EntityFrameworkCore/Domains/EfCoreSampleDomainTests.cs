using OVO.Samples;
using Xunit;

namespace OVO.EntityFrameworkCore.Domains;

[Collection(OVOTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<OVOEntityFrameworkCoreTestModule>
{

}
