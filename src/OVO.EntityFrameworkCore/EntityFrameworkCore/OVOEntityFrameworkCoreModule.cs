using System;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Uow;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using OVO.Community;
using OVO.EntityFrameworkCore.Community;
using OVO.EntityFrameworkCore.Moderation;
using OVO.EntityFrameworkCore.Outfits;
using OVO.EntityFrameworkCore.Social;
using OVO.EntityFrameworkCore.TryOn;
using OVO.EntityFrameworkCore.Users;
using OVO.EntityFrameworkCore.Wardrobe;
using OVO.EntityFrameworkCore.Wishlist;
using OVO.Moderation;
using OVO.Outfits;
using OVO.Social;
using OVO.TryOn;
using OVO.Users;
using OVO.Wardrobe;
using OVO.Wishlist;

namespace OVO.EntityFrameworkCore;

[DependsOn(
    typeof(OVODomainModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpOpenIddictEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AbpTenantManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule)
    )]
public class OVOEntityFrameworkCoreModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        OVOEfCoreEntityExtensionMappings.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<OVODbContext>(options =>
        {
                /* Remove "includeAllEntities: true" to create
                 * default repositories only for aggregate roots */
            options.AddDefaultRepositories(includeAllEntities: true);
            options.AddRepository<Garment, GarmentRepository>();
            options.AddRepository<UserProfile, UserProfileRepository>();
            options.AddRepository<UserPhoto, UserPhotoRepository>();
            options.AddRepository<Outfit, OutfitRepository>();
            options.AddRepository<Poll, PollRepository>();
            options.AddRepository<PollVote, PollVoteRepository>();
            options.AddRepository<UserFollow, UserFollowRepository>();
            options.AddRepository<UserBlock, UserBlockRepository>();
            options.AddRepository<WishlistItem, WishlistItemRepository>();
            options.AddRepository<ContentReport, ContentReportRepository>();
            options.AddRepository<RenderCache, RenderCacheRepository>();
        });

        Configure<AbpDbContextOptions>(options =>
        {
                /* The main point to change your DBMS.
                 * See also OVOMigrationsDbContextFactory for EF Core tooling. */
            options.UseNpgsql();
        });

    }
}
