using Microsoft.Extensions.Localization;
using OVO.Localization;
using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace OVO.Web;

[Dependency(ReplaceServices = true)]
public class OVOBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<OVOResource> _localizer;

    public OVOBrandingProvider(IStringLocalizer<OVOResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
