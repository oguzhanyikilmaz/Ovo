using Microsoft.AspNetCore.Builder;
using OVO;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();

builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("OVO.Web.csproj");
await builder.RunAbpModuleAsync<OVOWebTestModule>(applicationName: "OVO.Web" );

public partial class Program
{
}
