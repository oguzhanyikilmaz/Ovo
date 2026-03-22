using Volo.Abp.Settings;

namespace OVO.Settings;

public class OVOSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(OVOSettings.MySetting1));
    }
}
