using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace OVO.Pages;

public class Index_Tests : OVOWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
