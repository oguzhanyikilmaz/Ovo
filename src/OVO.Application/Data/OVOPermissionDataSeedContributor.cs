using System.Threading.Tasks;
using OVO.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.PermissionManagement;

namespace OVO.Data;

public class OVOPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string RolePermissionProviderName = "R";

    private static readonly string[] AllOvoPermissions =
    {
        OVOPermissions.Wardrobe.Default,
        OVOPermissions.Profile.Default,
        OVOPermissions.Outfits.Default,
        OVOPermissions.Trends.Default,
        OVOPermissions.Community.Default,
        OVOPermissions.Social.Default,
        OVOPermissions.TryOn.Default,
        OVOPermissions.Wishlist.Default,
        OVOPermissions.Reports.Create,
        OVOPermissions.FileStorage.Default
    };

    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public OVOPermissionDataSeedContributor(IPermissionDataSeeder permissionDataSeeder)
    {
        _permissionDataSeeder = permissionDataSeeder;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await _permissionDataSeeder.SeedAsync(
            RolePermissionProviderName,
            "admin",
            AllOvoPermissions);
    }
}
