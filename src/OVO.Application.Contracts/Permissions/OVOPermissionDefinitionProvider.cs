using OVO.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace OVO.Permissions;

public class OVOPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(OVOPermissions.GroupName);
        myGroup.AddPermission(OVOPermissions.Wardrobe.Default, L("Permission:Wardrobe"));
        myGroup.AddPermission(OVOPermissions.Profile.Default, L("Permission:Profile"));
        myGroup.AddPermission(OVOPermissions.Outfits.Default, L("Permission:Outfits"));
        myGroup.AddPermission(OVOPermissions.Trends.Default, L("Permission:Trends"));
        myGroup.AddPermission(OVOPermissions.Community.Default, L("Permission:Community"));
        myGroup.AddPermission(OVOPermissions.Social.Default, L("Permission:Social"));
        myGroup.AddPermission(OVOPermissions.TryOn.Default, L("Permission:TryOn"));
        myGroup.AddPermission(OVOPermissions.Wishlist.Default, L("Permission:Wishlist"));
        myGroup.AddPermission(OVOPermissions.Reports.Create, L("Permission:Reports"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<OVOResource>(name);
    }
}
