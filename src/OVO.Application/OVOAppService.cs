using System;
using System.Collections.Generic;
using System.Text;
using OVO.Localization;
using Volo.Abp.Application.Services;

namespace OVO;

/* Inherit your application services from this class.
 */
public abstract class OVOAppService : ApplicationService
{
    protected OVOAppService()
    {
        LocalizationResource = typeof(OVOResource);
    }
}
