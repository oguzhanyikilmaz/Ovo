using Microsoft.EntityFrameworkCore;
using OVO.Community;
using OVO.Moderation;
using OVO.Outfits;
using OVO.Social;
using OVO.TryOn;
using OVO.Users;
using OVO.Wardrobe;
using OVO.Wishlist;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace OVO.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class OVODbContext :
    AbpDbContext<OVODbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    public DbSet<Garment> Garments { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<UserPhoto> UserPhotos { get; set; }

    public DbSet<Outfit> Outfits { get; set; }

    public DbSet<Poll> Polls { get; set; }

    public DbSet<PollVote> PollVotes { get; set; }

    public DbSet<UserFollow> UserFollows { get; set; }

    public DbSet<UserBlock> UserBlocks { get; set; }

    public DbSet<WishlistItem> WishlistItems { get; set; }

    public DbSet<ContentReport> ContentReports { get; set; }

    public DbSet<RenderCache> RenderCaches { get; set; }

    #region Entities from the modules

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public OVODbContext(DbContextOptions<OVODbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        builder.Entity<Garment>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "Garments", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.SubCategory).IsRequired().HasMaxLength(128);
            b.Property(x => x.Color).IsRequired().HasMaxLength(64);
            b.Property(x => x.Pattern).IsRequired().HasMaxLength(64);
            b.Property(x => x.Seasons).IsRequired().HasMaxLength(256);
            b.Property(x => x.OriginalImageUrl).IsRequired().HasMaxLength(2048);
            b.Property(x => x.CutoutImageUrl).IsRequired().HasMaxLength(2048);
            b.Property(x => x.Size).HasMaxLength(32);
            b.Property(x => x.Notes).HasMaxLength(500);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.Category);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.Category });
        });

        builder.Entity<UserProfile>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "UserProfiles", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Gender).HasMaxLength(32);
            b.Property(x => x.StudioPhotoUrl).HasMaxLength(2048);
            b.Property(x => x.BodyType).HasMaxLength(64);
            b.HasIndex(x => x.TenantId);
        });

        builder.Entity<UserPhoto>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "UserPhotos", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.PhotoUrl).IsRequired().HasMaxLength(2048);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.TenantId, x.UserId });
        });

        builder.Entity<Outfit>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "Outfits", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.GarmentIdsJson).IsRequired().HasMaxLength(4000);
            b.Property(x => x.ComboHash).HasMaxLength(128);
            b.Property(x => x.HarmonyScore).HasPrecision(18, 4);
            b.Property(x => x.RenderUrl).HasMaxLength(2048);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.ComboHash);
            b.HasIndex(x => new { x.TenantId, x.UserId });
        });

        builder.Entity<Poll>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "Polls", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Question).IsRequired().HasMaxLength(512);
            b.Property(x => x.OptionsJson).IsRequired().HasMaxLength(8000);
            b.Property(x => x.OptionVoteCountsJson).IsRequired().HasMaxLength(512);
            b.HasIndex(x => x.CreatorUserId);
            b.HasIndex(x => new { x.TenantId, x.IsPublished });
        });

        builder.Entity<PollVote>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "PollVotes", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.PollId, x.UserId }).IsUnique();
            b.HasIndex(x => x.TenantId);
        });

        builder.Entity<UserFollow>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "UserFollows", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.FollowerId, x.FollowingId }).IsUnique();
            b.HasIndex(x => x.TenantId);
        });

        builder.Entity<UserBlock>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "UserBlocks", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasIndex(x => new { x.BlockerId, x.BlockedId }).IsUnique();
            b.HasIndex(x => x.TenantId);
        });

        builder.Entity<WishlistItem>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "WishlistItems", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContentId).IsRequired().HasMaxLength(128);
            b.Property(x => x.SourceType).HasMaxLength(64);
            b.Property(x => x.SourceLabel).HasMaxLength(256);
            b.Property(x => x.PreviewImageUrl).HasMaxLength(2048);
            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.ContentType, x.ContentId }).IsUnique();
        });

        builder.Entity<ContentReport>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "ContentReports", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(64);
            b.Property(x => x.ContentId).IsRequired().HasMaxLength(128);
            b.Property(x => x.Reason).IsRequired().HasMaxLength(1024);
            b.HasIndex(x => x.ReporterUserId);
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        builder.Entity<RenderCache>(b =>
        {
            b.ToTable(OVOConsts.DbTablePrefix + "RenderCaches", OVOConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RenderUrl).IsRequired().HasMaxLength(2048);
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.TenantId);
        });
    }
}
