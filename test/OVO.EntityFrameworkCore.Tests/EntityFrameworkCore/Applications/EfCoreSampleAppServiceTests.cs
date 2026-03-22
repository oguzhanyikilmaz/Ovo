using OVO.Samples;
using Xunit;

namespace OVO.EntityFrameworkCore.Applications;

[Collection(OVOTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<OVOEntityFrameworkCoreTestModule>
{

}
