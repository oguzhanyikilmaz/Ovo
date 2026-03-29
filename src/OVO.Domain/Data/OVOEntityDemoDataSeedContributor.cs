using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OVO.Community;
using OVO.Moderation;
using OVO.Outfits;
using OVO.Social;
using OVO.TryOn;
using OVO.Users;
using OVO.Wardrobe;
using OVO.Wishlist;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace OVO.Data;

/// <summary>
/// OVO domain entity'leri için demo veri (görsel URL'leri yalnızca yer tutucu).
/// Kimlik kullanıcıları oluşturulduktan sonra çalışır; tekrar çalıştırılabilir (idempotent).
/// </summary>
public class OVOEntityDemoDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    public const string SeedMarker = "__ovo_demo_seed_v1";

    private const string DemoPassword = "DemoUser123!@#";

    /// <summary>Demo kimlik kullanıcıları (Id sabit; yeniden seed güvenli).</summary>
    private static readonly Guid[] DemoUserIds =
    {
        Guid.Parse("a1111111-1111-4111-8111-111111111101"),
        Guid.Parse("a1111111-1111-4111-8111-111111111102"),
        Guid.Parse("a1111111-1111-4111-8111-111111111103"),
        Guid.Parse("a1111111-1111-4111-8111-111111111104"),
        Guid.Parse("a1111111-1111-4111-8111-111111111105")
    };

    private static readonly Guid[] GarmentIds =
    {
        Guid.Parse("b2222222-2222-4222-8222-222222222201"),
        Guid.Parse("b2222222-2222-4222-8222-222222222202"),
        Guid.Parse("b2222222-2222-4222-8222-222222222203"),
        Guid.Parse("b2222222-2222-4222-8222-222222222204"),
        Guid.Parse("b2222222-2222-4222-8222-222222222205")
    };

    private static readonly Guid[] OutfitIds =
    {
        Guid.Parse("c3333333-3333-4333-8333-333333333301"),
        Guid.Parse("c3333333-3333-4333-8333-333333333302"),
        Guid.Parse("c3333333-3333-4333-8333-333333333303"),
        Guid.Parse("c3333333-3333-4333-8333-333333333304"),
        Guid.Parse("c3333333-3333-4333-8333-333333333305")
    };

    private static readonly Guid[] PollIds =
    {
        Guid.Parse("d4444444-4444-4444-8444-444444444401"),
        Guid.Parse("d4444444-4444-4444-8444-444444444402"),
        Guid.Parse("d4444444-4444-4444-8444-444444444403"),
        Guid.Parse("d4444444-4444-4444-8444-444444444404"),
        Guid.Parse("d4444444-4444-4444-8444-444444444405")
    };

    private static readonly Guid[] PollVoteIds =
    {
        Guid.Parse("e5555555-5555-4555-8555-555555555501"),
        Guid.Parse("e5555555-5555-4555-8555-555555555502"),
        Guid.Parse("e5555555-5555-4555-8555-555555555503"),
        Guid.Parse("e5555555-5555-4555-8555-555555555504"),
        Guid.Parse("e5555555-5555-4555-8555-555555555505")
    };

    private static readonly Guid[] UserPhotoIds =
    {
        Guid.Parse("f6666666-6666-4666-8666-666666666601"),
        Guid.Parse("f6666666-6666-4666-8666-666666666602"),
        Guid.Parse("f6666666-6666-4666-8666-666666666603"),
        Guid.Parse("f6666666-6666-4666-8666-666666666604"),
        Guid.Parse("f6666666-6666-4666-8666-666666666605")
    };

    private static readonly Guid[] UserFollowIds =
    {
        Guid.Parse("07777777-7777-4777-8777-777777777701"),
        Guid.Parse("07777777-7777-4777-8777-777777777702"),
        Guid.Parse("07777777-7777-4777-8777-777777777703"),
        Guid.Parse("07777777-7777-4777-8777-777777777704"),
        Guid.Parse("07777777-7777-4777-8777-777777777705")
    };

    private static readonly Guid[] UserBlockIds =
    {
        Guid.Parse("08888888-8888-4888-8888-888888888801"),
        Guid.Parse("08888888-8888-4888-8888-888888888802"),
        Guid.Parse("08888888-8888-4888-8888-888888888803"),
        Guid.Parse("08888888-8888-4888-8888-888888888804"),
        Guid.Parse("08888888-8888-4888-8888-888888888805")
    };

    private static readonly Guid[] WishlistIds =
    {
        Guid.Parse("09999999-9999-4999-8999-999999999901"),
        Guid.Parse("09999999-9999-4999-8999-999999999902"),
        Guid.Parse("09999999-9999-4999-8999-999999999903"),
        Guid.Parse("09999999-9999-4999-8999-999999999904"),
        Guid.Parse("09999999-9999-4999-8999-999999999905")
    };

    private static readonly Guid[] ContentReportIds =
    {
        Guid.Parse("0aaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa01"),
        Guid.Parse("0aaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa02"),
        Guid.Parse("0aaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa03"),
        Guid.Parse("0aaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa04"),
        Guid.Parse("0aaaaaaa-aaaa-4aaa-8aaa-aaaaaaaaaa05")
    };

    private readonly IGarmentRepository _garmentRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserPhotoRepository _userPhotoRepository;
    private readonly IOutfitRepository _outfitRepository;
    private readonly IPollRepository _pollRepository;
    private readonly IPollVoteRepository _pollVoteRepository;
    private readonly IUserFollowRepository _userFollowRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private readonly IWishlistItemRepository _wishlistItemRepository;
    private readonly IContentReportRepository _contentReportRepository;
    private readonly IRenderCacheRepository _renderCacheRepository;
    private readonly IdentityUserManager _identityUserManager;

    public OVOEntityDemoDataSeedContributor(
        IGarmentRepository garmentRepository,
        IUserProfileRepository userProfileRepository,
        IUserPhotoRepository userPhotoRepository,
        IOutfitRepository outfitRepository,
        IPollRepository pollRepository,
        IPollVoteRepository pollVoteRepository,
        IUserFollowRepository userFollowRepository,
        IUserBlockRepository userBlockRepository,
        IWishlistItemRepository wishlistItemRepository,
        IContentReportRepository contentReportRepository,
        IRenderCacheRepository renderCacheRepository,
        IdentityUserManager identityUserManager)
    {
        _garmentRepository = garmentRepository;
        _userProfileRepository = userProfileRepository;
        _userPhotoRepository = userPhotoRepository;
        _outfitRepository = outfitRepository;
        _pollRepository = pollRepository;
        _pollVoteRepository = pollVoteRepository;
        _userFollowRepository = userFollowRepository;
        _userBlockRepository = userBlockRepository;
        _wishlistItemRepository = wishlistItemRepository;
        _contentReportRepository = contentReportRepository;
        _renderCacheRepository = renderCacheRepository;
        _identityUserManager = identityUserManager;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        var tenantId = context.TenantId;
        if (await _garmentRepository.AnyAsync(g => g.Notes == SeedMarker && g.TenantId == tenantId))
        {
            return;
        }

        await EnsureDemoUsersAsync(context);

        var users = DemoUserIds.ToList();
        var tenantKey = tenantId.HasValue ? tenantId.Value.ToString("N")[..8] : "host";

        // 1) Profiller (Id = IdentityUser.Id)
        for (var i = 0; i < 5; i++)
        {
            var uid = users[i];
            if (await _userProfileRepository.FindAsync(uid) != null)
            {
                continue;
            }

            var genders = new[] { "Kadın", "Erkek", "Kadın", "Erkek", "Unisex" };
            var packages = new[] { UserPackage.Free, UserPackage.Free, UserPackage.Premium, UserPackage.Free, UserPackage.Free };
            var profile = new UserProfile(uid)
            {
                TenantId = tenantId,
                Gender = genders[i],
                Package = packages[i],
                DailyRenderCount = i,
                HeightCm = 160 + i * 2,
                WeightKg = 55 + i * 1.5,
                BodyType = i % 2 == 0 ? "Dörtgen" : "Üçgen",
                AccountStatus = AccountStatus.Active
            };
            await _userProfileRepository.InsertAsync(profile, autoSave: true);
        }

        // 2) Kullanıcı fotoğrafları (URL yer tutucu)
        for (var i = 0; i < 5; i++)
        {
            if (await _userPhotoRepository.FindAsync(UserPhotoIds[i]) != null)
            {
                continue;
            }

            var photo = new UserPhoto(
                UserPhotoIds[i],
                users[i],
                $"https://placeholder.ovo.local/seed/photo/{tenantKey}/{UserPhotoIds[i]:N}.jpg",
                tenantId,
                qualityScore: 0.75 + i * 0.04,
                hasFace: true,
                isFullBody: i % 2 == 0);
            await _userPhotoRepository.InsertAsync(photo, autoSave: true);
        }

        // 3) Kıyafetler (her biri farklı kullanıcıya; görsel alanları yer tutucu)
        var categories = new[]
        {
            GarmentCategory.Top, GarmentCategory.Bottom, GarmentCategory.Shoes, GarmentCategory.Outer,
            GarmentCategory.Accessory
        };
        var subCats = new[] { "Gömlek", "Chino", "Loafer", "Trençkot", "Saat" };
        var colors = new[] { "Lacivert", "Bej", "Siyah", "Camel", "Gümüş" };
        var patterns = new[] { "Düz", "Çizgili", "Düz", "Ekose", "Düz" };
        var seasons = new[] { "İlkbahar,Yaz", "Yaz", "DörtMevsim", "Sonbahar,Kış", "Yaz" };
        var formalities = new[]
        {
            GarmentFormality.SmartCasual, GarmentFormality.Casual, GarmentFormality.Business,
            GarmentFormality.Elegant, GarmentFormality.Casual
        };
        var sources = new[]
        {
            GarmentSource.Upload, GarmentSource.Url, GarmentSource.Photo, GarmentSource.Upload, GarmentSource.Url
        };

        for (var i = 0; i < 5; i++)
        {
            if (await _garmentRepository.FindAsync(GarmentIds[i]) != null)
            {
                continue;
            }

            var g = new Garment(
                GarmentIds[i],
                users[i],
                categories[i],
                subCats[i],
                colors[i],
                patterns[i],
                seasons[i],
                formalities[i],
                GarmentVisibility.Visible,
                sources[i],
                $"https://placeholder.ovo.local/seed/garment/{GarmentIds[i]:N}/original.jpg",
                $"https://placeholder.ovo.local/seed/garment/{GarmentIds[i]:N}/cutout.png",
                size: $"{46 + i}",
                notes: $"{SeedMarker} — demo #{i + 1}",
                tenantId: tenantId);
            await _garmentRepository.InsertAsync(g, autoSave: true);
        }

        // 4) Kombinler (her kullanıcı kendi kıyafetiyle)
        var visibilities = new[]
        {
            OutfitVisibility.Visible, OutfitVisibility.Visible, OutfitVisibility.Hidden, OutfitVisibility.Visible,
            OutfitVisibility.Visible
        };
        for (var i = 0; i < 5; i++)
        {
            if (await _outfitRepository.FindAsync(OutfitIds[i]) != null)
            {
                continue;
            }

            var json = JsonSerializer.Serialize(new[] { GarmentIds[i] });
            var outfit = new Outfit(
                OutfitIds[i],
                users[i],
                json,
                visibilities[i],
                tenantId,
                comboHash: $"demo-combo-{tenantKey}-{i}",
                harmonyScore: 0.65m + 0.05m * i,
                renderUrl: null)
            {
                IsShared = i % 2 == 0
            };
            await _outfitRepository.InsertAsync(outfit, autoSave: true);
        }

        // 5) Oylamalar
        for (var i = 0; i < 5; i++)
        {
            if (await _pollRepository.FindAsync(PollIds[i]) != null)
            {
                continue;
            }

            var optionsJson = JsonSerializer.Serialize(new[]
            {
                new Dictionary<string, string> { ["label"] = "Seçenek A", ["imageUrl"] = "" },
                new Dictionary<string, string> { ["label"] = "Seçenek B", ["imageUrl"] = "" }
            });
            var poll = new Poll(
                PollIds[i],
                users[i],
                $"Hangisini giysem? (demo #{i + 1})",
                optionsJson,
                "[1,0]",
                isPublished: true,
                tenantId: tenantId)
            {
                TotalVotes = 1
            };
            await _pollRepository.InsertAsync(poll, autoSave: true);
        }

        // 6) Oylar (aynı anket + kullanıcı çifti tekil; oylayan, oluşturandan farklı)
        var voterOffsets = new[] { 1, 2, 3, 4, 0 };
        for (var i = 0; i < 5; i++)
        {
            if (await _pollVoteRepository.FindAsync(PollVoteIds[i]) != null)
            {
                continue;
            }

            var voter = users[voterOffsets[i]];
            var vote = new PollVote(PollVoteIds[i], PollIds[i], voter, optionIndex: i % 2, tenantId: tenantId);
            await _pollVoteRepository.InsertAsync(vote, autoSave: true);
        }

        // 7) Takip (halka: U1→U2→U3→U4→U5→U1)
        var followChain = new[] { (0, 1), (1, 2), (2, 3), (3, 4), (4, 0) };
        for (var i = 0; i < 5; i++)
        {
            if (await _userFollowRepository.FindAsync(UserFollowIds[i]) != null)
            {
                continue;
            }

            var (a, b) = followChain[i];
            var follow = new UserFollow(UserFollowIds[i], users[a], users[b], tenantId);
            await _userFollowRepository.InsertAsync(follow, autoSave: true);
        }

        // 8) Engellemeler (takipten bağımsız çiftler)
        var blockPairs = new[] { (0, 3), (1, 4), (2, 0), (3, 1), (4, 2) };
        for (var i = 0; i < 5; i++)
        {
            if (await _userBlockRepository.FindAsync(UserBlockIds[i]) != null)
            {
                continue;
            }

            var (blocker, blocked) = blockPairs[i];
            var block = new UserBlock(UserBlockIds[i], users[blocker], users[blocked], tenantId);
            await _userBlockRepository.InsertAsync(block, autoSave: true);
        }

        // 9) Wishlist (ContentType + ContentId tekil)
        var wishlistSpecs = new[]
        {
            (WishlistContentType.Poll, PollIds[0].ToString("D"), "Topluluk"),
            (WishlistContentType.Outfit, OutfitIds[1].ToString("D"), "Kombin"),
            (WishlistContentType.TryOnRender, "tryon-demo-001", "Try-On"),
            (WishlistContentType.TrendOutfit, "trend-demo-042", "Trend"),
            (WishlistContentType.Poll, PollIds[4].ToString("D"), "Topluluk")
        };
        for (var i = 0; i < 5; i++)
        {
            if (await _wishlistItemRepository.FindAsync(WishlistIds[i]) != null)
            {
                continue;
            }

            var (ctype, cid, label) = wishlistSpecs[i];
            var item = new WishlistItem(
                WishlistIds[i],
                users[i],
                ctype,
                cid,
                tenantId,
                sourceType: "seed",
                sourceLabel: label,
                previewImageUrl: null);
            await _wishlistItemRepository.InsertAsync(item, autoSave: true);
        }

        // 10) İçerik raporları
        var reportTargets = new[]
        {
            ("poll", PollIds[0].ToString("D")),
            ("outfit", OutfitIds[0].ToString("D")),
            ("comment", "demo-comment-1"),
            ("profile", users[2].ToString("D")),
            ("poll", PollIds[1].ToString("D"))
        };
        var statuses = new[]
        {
            ReportStatus.Pending, ReportStatus.Reviewed, ReportStatus.Dismissed, ReportStatus.Pending,
            ReportStatus.Reviewed
        };
        for (var i = 0; i < 5; i++)
        {
            if (await _contentReportRepository.FindAsync(ContentReportIds[i]) != null)
            {
                continue;
            }

            var (ctype, cid) = reportTargets[i];
            var report = new ContentReport(
                ContentReportIds[i],
                users[i],
                ctype,
                cid,
                $"Demo rapor açıklaması #{i + 1} — {SeedMarker}",
                tenantId)
            {
                Status = statuses[i]
            };
            await _contentReportRepository.InsertAsync(report, autoSave: true);
        }

        // 11) Render önbelleği (string Id = kombin hash)
        for (var i = 0; i < 5; i++)
        {
            var hash = $"ovo-demo-render-{tenantKey}-{i:00}";
            if (await _renderCacheRepository.FindAsync(hash) != null)
            {
                continue;
            }

            var cache = new RenderCache(
                hash,
                $"https://placeholder.ovo.local/seed/render/{tenantKey}/{i}.jpg",
                tenantId);
            await _renderCacheRepository.InsertAsync(cache, autoSave: true);
        }
    }

    private async Task EnsureDemoUsersAsync(DataSeedContext context)
    {
        var tenantId = context.TenantId;
        var tenantKey = tenantId?.ToString("N") ?? "host";

        for (var i = 0; i < DemoUserIds.Length; i++)
        {
            var id = DemoUserIds[i];
            if (await _identityUserManager.FindByIdAsync(id.ToString()) != null)
            {
                continue;
            }

            var n = i + 1;
            var userName = $"ovo_demo_u{n}_{tenantKey[..Math.Min(tenantKey.Length, 16)]}";
            var email = $"ovo.demo.u{n}.{tenantKey}@seed.ovo.local";

            var user = new IdentityUser(id, userName, email, tenantId);
            user.SetEmailConfirmed(true);
            user.SetIsActive(true);

            var result = await _identityUserManager.CreateAsync(user, DemoPassword);
            if (!result.Succeeded)
            {
                var existing = await _identityUserManager.FindByEmailAsync(email);
                if (existing != null)
                {
                    continue;
                }

                throw new InvalidOperationException(
                    "OVO demo kullanıcısı oluşturulamadı: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
