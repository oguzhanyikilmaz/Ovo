using OVO.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace OVO.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class OVOPageModel : AbpPageModel
{
    protected OVOPageModel()
    {
        LocalizationResourceType = typeof(OVOResource);
    }
}
