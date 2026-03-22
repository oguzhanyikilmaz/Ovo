using OVO.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace OVO.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class OVOController : AbpControllerBase
{
    protected OVOController()
    {
        LocalizationResource = typeof(OVOResource);
    }
}
