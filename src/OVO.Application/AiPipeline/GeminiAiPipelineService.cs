using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OVO;
using OVO.FileStorage;
using OVO.Wardrobe;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace OVO.AiPipeline;

/// <summary>
/// Google Gemini REST API: Nano Banana Pro (<see cref="GeminiOptions.ImageModel"/>) ile stüdyo / try-on görseli;
/// görüntü anlama için <see cref="GeminiOptions.VisionModel"/>.
/// </summary>
public sealed class GeminiAiPipelineService : IAiPipelineService
{
    public const string HttpClientName = "OvoGemini";

    private static readonly JsonSerializerOptions JsonCamel = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions StructuredBriefJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private const string DefaultStudioPhotoPrompt =
        "Create a single full-body fashion studio photograph of the same person as in the reference photo(s). " +
        "Plain white background, natural relaxed standing pose, soft studio lighting, realistic skin and proportions. " +
        "Vertical framing suitable for virtual try-on. No text or watermark.";

    private const string DefaultTryOnPrompt =
        "Professional virtual try-on: the person from the first image wears the clothing item(s) shown in the following image(s). " +
        "Realistic fit, consistent lighting and shadows, full body, plain neutral background, high-end fashion catalog look. " +
        "9:16 vertical. No text or logos added.";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiOptions _options;
    private readonly IObjectStorageService _objectStorage;
    private readonly MinioStorageOptions _minio;
    private readonly IGarmentRepository _garmentRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ILogger<GeminiAiPipelineService> _logger;

    public GeminiAiPipelineService(
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiOptions> geminiOptions,
        IOptions<MinioStorageOptions> minioOptions,
        IObjectStorageService objectStorage,
        IGarmentRepository garmentRepository,
        IAsyncQueryableExecuter asyncExecuter,
        ILogger<GeminiAiPipelineService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = geminiOptions.Value;
        _minio = minioOptions.Value;
        _objectStorage = objectStorage;
        _garmentRepository = garmentRepository;
        _asyncExecuter = asyncExecuter;
        _logger = logger;
    }

    public async Task<bool> CheckNsfwAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        using var opCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        opCts.CancelAfter(TimeSpan.FromSeconds(_options.VisionTimeoutSeconds));

        var (mime, b64) = await DownloadImageAsBase64Async(imageUrl, opCts.Token).ConfigureAwait(false);
        var prompt =
            "You are a content safety reviewer for a fashion app. " +
            "Look at the image. Is it safe for a general audience (no nudity, no sexual content, no violence, no hate)? " +
            "Answer with a single JSON object only, no markdown: {\"safe\":true} or {\"safe\":false}.";

        var text = await GenerateVisionTextAsync(prompt, mime, b64, opCts.Token).ConfigureAwait(false);
        if (TryParseSafeJson(text, out var safe))
        {
            return safe;
        }

        _logger.LogWarning("Gemini NSFW yanıtı ayrıştırılamadı: {Snippet}", Truncate(text, 200));
        return false;
    }

    public async Task<GarmentImageProcessResult> ProcessGarmentImageAsync(
        string imageUrl,
        CancellationToken cancellationToken = default)
    {
        using var opCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        opCts.CancelAfter(TimeSpan.FromSeconds(_options.ImageGenerationTimeoutSeconds));

        var (mime, b64) = await DownloadImageAsBase64Async(imageUrl, opCts.Token).ConfigureAwait(false);

        var tagPrompt =
            "Analyze this clothing / fashion item image. Reply with ONE JSON object only (no markdown): " +
            "{\"category\":\"Top|Bottom|Outer|Shoes|Accessory\",\"subCategory\":\"short Turkish label\"," +
            "\"color\":\"Turkish\",\"pattern\":\"Düz|Çizgili|Ekose|Çiçekli|Diğer\"," +
            "\"seasons\":\"comma-separated Turkish seasons e.g. İlkbahar,Yaz\"," +
            "\"formality\":\"Casual|SmartCasual|Business|Elegant|Sport\"}";

        var tagJson = await GenerateVisionTextAsync(tagPrompt, mime, b64, opCts.Token).ConfigureAwait(false);
        var tags = ParseGarmentTags(tagJson);

        var cutoutPrompt =
            "Fashion e-commerce product shot: isolate the main garment from the input image. " +
            "Clean studio lighting, the garment only, centered, on pure white (#FFFFFF) background, no mannequin if possible, photorealistic.";

        var pngBytes = await GenerateImageBytesAsync(
            cutoutPrompt,
            new[] { (mime, b64) },
            aspectRatio: "1:1",
            imageSize: "1K",
            opCts.Token).ConfigureAwait(false);

        var cutoutUrl = await UploadPngAsync("garment-cutouts", pngBytes, opCts.Token).ConfigureAwait(false);

        return new GarmentImageProcessResult
        {
            CutoutImageUrl = cutoutUrl,
            Category = tags.Category,
            SubCategory = tags.SubCategory,
            Color = tags.Color,
            Pattern = tags.Pattern,
            Seasons = tags.Seasons,
            Formality = tags.Formality
        };
    }

    public async Task<string?> ExtractProductImageFromUrlAsync(string pageUrl, CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (compatible; OvoBot/1.0; +https://ovo.local)");

        string html;
        try
        {
            html = await client.GetStringAsync(new Uri(pageUrl), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Ürün sayfası indirilemedi: {Url}", pageUrl);
            return null;
        }

        var abs = TryExtractOgImage(html, pageUrl);
        return abs;
    }

    public async Task<string> GenerateStudioPhotoAsync(
        IReadOnlyList<string> sourcePhotoUrls,
        CancellationToken cancellationToken = default)
    {
        if (sourcePhotoUrls.Count == 0)
        {
            throw new UserFriendlyException("Stüdyo foto için en az bir kaynak görsel gerekli.");
        }

        using var opCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        opCts.CancelAfter(TimeSpan.FromSeconds(_options.ImageGenerationTimeoutSeconds));

        var refs = new List<(string Mime, string Base64)>();
        var max = Math.Min(sourcePhotoUrls.Count, 6);
        for (var i = 0; i < max; i++)
        {
            refs.Add(await DownloadImageAsBase64Async(sourcePhotoUrls[i], opCts.Token).ConfigureAwait(false));
        }

        var studio = _options.StudioPhoto;
        var prompt = BuildStudioPhotoPrompt(studio);
        var aspect = string.IsNullOrWhiteSpace(studio.AspectRatio) ? "9:16" : studio.AspectRatio.Trim();

        var pngBytes = await GenerateImageBytesAsync(
            prompt,
            refs,
            aspectRatio: aspect,
            imageSize: studio.ImageSize,
            opCts.Token).ConfigureAwait(false);

        return await UploadPngAsync("studio", pngBytes, opCts.Token).ConfigureAwait(false);
    }

    public async Task<string> RenderTryOnAsync(
        string studioPhotoUrl,
        IReadOnlyList<string> garmentCutoutUrls,
        CancellationToken cancellationToken = default)
    {
        using var opCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        opCts.CancelAfter(TimeSpan.FromSeconds(_options.ImageGenerationTimeoutSeconds));

        var refs = new List<(string Mime, string Base64)>
        {
            await DownloadImageAsBase64Async(studioPhotoUrl, opCts.Token).ConfigureAwait(false)
        };

        foreach (var url in garmentCutoutUrls.Take(10))
        {
            refs.Add(await DownloadImageAsBase64Async(url, opCts.Token).ConfigureAwait(false));
        }

        var tryOn = _options.TryOn;
        var prompt = BuildTryOnPrompt(tryOn);
        var aspect = string.IsNullOrWhiteSpace(tryOn.AspectRatio) ? "9:16" : tryOn.AspectRatio.Trim();

        var pngBytes = await GenerateImageBytesAsync(
            prompt,
            refs,
            aspectRatio: aspect,
            imageSize: tryOn.ImageSize,
            opCts.Token).ConfigureAwait(false);

        return await UploadPngAsync("tryon-renders", pngBytes, opCts.Token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Guid>> SuggestOutfitGarmentIdsAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 20);
        var q = await _garmentRepository.GetQueryableAsync().ConfigureAwait(false);
        var query = q
            .Where(g => g.UserId == userId && g.Visibility == GarmentVisibility.Visible)
            .OrderByDescending(g => g.CreationTime)
            .Take(take);

        var list = await _asyncExecuter.ToListAsync(query, cancellationToken).ConfigureAwait(false);
        return list.Select(g => g.Id).ToList();
    }

    public async Task<IReadOnlyList<Guid>> AutoCompleteOutfitAsync(
        IReadOnlyList<Guid> selectedGarmentIds,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var q = await _garmentRepository.GetQueryableAsync().ConfigureAwait(false);
        var wardrobe = await _asyncExecuter.ToListAsync(
            q.Where(g => g.UserId == userId && g.Visibility == GarmentVisibility.Visible),
            cancellationToken).ConfigureAwait(false);

        if (wardrobe.Count == 0)
        {
            return selectedGarmentIds.ToList();
        }

        var selected = wardrobe.Where(g => selectedGarmentIds.Contains(g.Id)).ToList();
        var result = selectedGarmentIds.ToList();
        var have = selected.Select(s => s.Category).ToHashSet();

        foreach (var cat in new[] { GarmentCategory.Top, GarmentCategory.Bottom, GarmentCategory.Shoes })
        {
            if (have.Contains(cat))
            {
                continue;
            }

            var pick = wardrobe.FirstOrDefault(g => g.Category == cat && !result.Contains(g.Id));
            if (pick != null)
            {
                result.Add(pick.Id);
                have.Add(cat);
            }
        }

        return result;
    }

    public async Task<UserPhotoAnalysisResult?> AnalyzeUserPhotoAsync(
        string photoUrl,
        CancellationToken cancellationToken = default)
    {
        using var opCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        opCts.CancelAfter(TimeSpan.FromSeconds(_options.VisionTimeoutSeconds));

        var (mime, b64) = await DownloadImageAsBase64Async(photoUrl, opCts.Token).ConfigureAwait(false);
        var prompt =
            "Assess this photo for a virtual try-on app. Reply ONE JSON only (no markdown): " +
            "{\"qualityScore\":0.0-1.0,\"hasFace\":true/false,\"isFullBody\":true/false} " +
            "qualityScore: sharpness and lighting; isFullBody: entire body from head to feet visible.";

        var text = await GenerateVisionTextAsync(prompt, mime, b64, opCts.Token).ConfigureAwait(false);
        return ParseUserPhotoAnalysis(text);
    }

    /// <summary>
    /// <see cref="GeminiStudioPhotoOptions.Prompt"/> doluysa aynen kullanılır (tam kontrol).
    /// Boşsa <see cref="GeminiStudioPhotoOptions.StructuredBrief"/> JSON metin olarak eklenir; o da boşsa varsayılan metin.
    /// </summary>
    private static string BuildStudioPhotoPrompt(GeminiStudioPhotoOptions studio)
    {
        if (!string.IsNullOrWhiteSpace(studio.Prompt))
        {
            return studio.Prompt.Trim();
        }

        if (studio.StructuredBrief != null && StudioBriefHasContent(studio.StructuredBrief))
        {
            var json = JsonSerializer.Serialize(studio.StructuredBrief, StructuredBriefJson);
            return
                "The attached photo(s) show the same person—use them as reference for identity, face, and body proportions.\n\n" +
                "Follow this creative brief (JSON). It defines background, lighting, camera, pose, realism, output, and constraints.\n\n" +
                json +
                "\n\nGenerate exactly one photorealistic full-body studio photograph matching the brief.";
        }

        return
            "The attached photo(s) show the same person—use them as reference for identity and body proportions.\n\n" +
            DefaultStudioPhotoPrompt;
    }

    private static bool StudioBriefHasContent(StudioPhotoStructuredBrief b) =>
        !string.IsNullOrWhiteSpace(b.Summary)
        || !string.IsNullOrWhiteSpace(b.Background)
        || !string.IsNullOrWhiteSpace(b.Lighting)
        || !string.IsNullOrWhiteSpace(b.CameraAndFraming)
        || !string.IsNullOrWhiteSpace(b.PoseAndExpression)
        || !string.IsNullOrWhiteSpace(b.SkinAndRealism)
        || !string.IsNullOrWhiteSpace(b.OutputFormat)
        || !string.IsNullOrWhiteSpace(b.MustAvoid);

    private static string BuildTryOnPrompt(GeminiTryOnOptions tryOn)
    {
        if (!string.IsNullOrWhiteSpace(tryOn.Prompt))
        {
            return tryOn.Prompt.Trim();
        }

        if (tryOn.StructuredBrief != null && TryOnBriefHasContent(tryOn.StructuredBrief))
        {
            var json = JsonSerializer.Serialize(tryOn.StructuredBrief, StructuredBriefJson);
            return
                "Image order: the FIRST attached image is the full-body studio photo of the person. " +
                "The FOLLOWING images are garment piece(s) (cutouts or flat product shots) to wear.\n\n" +
                "Follow this virtual try-on brief (JSON).\n\n" +
                json +
                "\n\nGenerate exactly one photorealistic full-body result: the person wearing the specified garment(s).";
        }

        return
            "Image order: the FIRST attached image is the full-body studio photo of the person; " +
            "the FOLLOWING images are garment item(s) to apply.\n\n" +
            DefaultTryOnPrompt;
    }

    private static bool TryOnBriefHasContent(TryOnStructuredBrief b) =>
        !string.IsNullOrWhiteSpace(b.Summary)
        || !string.IsNullOrWhiteSpace(b.ImageOrderHint)
        || !string.IsNullOrWhiteSpace(b.GarmentApplication)
        || !string.IsNullOrWhiteSpace(b.FitAndRealism)
        || !string.IsNullOrWhiteSpace(b.Lighting)
        || !string.IsNullOrWhiteSpace(b.Background)
        || !string.IsNullOrWhiteSpace(b.CameraAndFraming)
        || !string.IsNullOrWhiteSpace(b.OutputFormat)
        || !string.IsNullOrWhiteSpace(b.MustAvoid);

    private async Task<string> GenerateVisionTextAsync(
        string prompt,
        string imageMime,
        string imageBase64,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var url = BuildModelUrl(_options.VisionModel);
        var body = new GeminiGenerateContentRequest
        {
            Contents =
            [
                new GeminiContentRequest
                {
                    Role = "user",
                    Parts =
                    [
                        new GeminiPartRequest { Text = prompt },
                        new GeminiPartRequest
                        {
                            InlineData = new GeminiInlineDataRequest
                            {
                                MimeType = imageMime,
                                Data = imageBase64
                            }
                        }
                    ]
                }
            ]
        };

        var json = JsonSerializer.Serialize(body, JsonCamel);
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.TryAddWithoutValidation("x-goog-api-key", _options.ApiKey);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, cancellationToken).ConfigureAwait(false);
        var payload = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        EnsureOk(resp, payload);

        return ExtractFirstTextPart(payload) ?? string.Empty;
    }

    private async Task<byte[]> GenerateImageBytesAsync(
        string textPrompt,
        IReadOnlyList<(string Mime, string Base64)> referenceImages,
        string aspectRatio,
        string? imageSize,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var url = BuildModelUrl(_options.ImageModel);

        var parts = new List<GeminiPartRequest> { new() { Text = textPrompt } };
        foreach (var (mime, b64) in referenceImages)
        {
            parts.Add(new GeminiPartRequest
            {
                InlineData = new GeminiInlineDataRequest { MimeType = mime, Data = b64 }
            });
        }

        var body = new GeminiGenerateContentRequest
        {
            Contents =
            [
                new GeminiContentRequest { Role = "user", Parts = parts }
            ],
            GenerationConfig = new GeminiGenerationConfigRequest
            {
                ResponseModalities = ["IMAGE"],
                ImageConfig = new GeminiImageConfigRequest
                {
                    AspectRatio = aspectRatio,
                    ImageSize = imageSize
                }
            }
        };

        var json = JsonSerializer.Serialize(body, JsonCamel);
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        req.Headers.TryAddWithoutValidation("x-goog-api-key", _options.ApiKey);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using var resp = await client.SendAsync(req, cancellationToken).ConfigureAwait(false);
        var payload = await resp.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        EnsureOk(resp, payload);

        var bytes = ExtractFinalImageBytes(payload);
        if (bytes == null || bytes.Length == 0)
        {
            throw new UserFriendlyException("Görsel üretilemedi. Lütfen tekrar deneyin.");
        }

        return bytes;
    }

    private Uri BuildModelUrl(string model)
    {
        var root = _options.BaseUrl.TrimEnd('/') + "/";
        return new Uri(new Uri(root, UriKind.Absolute), $"v1beta/models/{model}:generateContent");
    }

    private void EnsureOk(HttpResponseMessage resp, string payload)
    {
        if (resp.IsSuccessStatusCode)
        {
            if (TryGetBlockReason(payload, out var reason))
            {
                _logger.LogWarning("Gemini prompt engellendi: {Reason}", reason);
                throw new UserFriendlyException("İstek güvenlik filtrelerinde engellendi. Farklı bir görsel deneyin.");
            }

            return;
        }

        _logger.LogError("Gemini HTTP {Status}: {Body}", (int)resp.StatusCode, Truncate(payload, 500));
        throw new BusinessException(OVODomainErrorCodes.GeminiApiError)
            .WithData("Status", (int)resp.StatusCode);
    }

    private async Task<(string Mime, string Base64)> DownloadImageAsBase64Async(
        string url,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        var bytes = await resp.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        if (bytes.Length == 0)
        {
            throw new UserFriendlyException("Görsel indirilemedi (boş yanıt).");
        }

        var mime = resp.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
        if (mime.StartsWith("text/", StringComparison.OrdinalIgnoreCase))
        {
            mime = "image/jpeg";
        }

        return (mime, Convert.ToBase64String(bytes));
    }

    private async Task<string> UploadPngAsync(string folder, byte[] pngBytes, CancellationToken cancellationToken)
    {
        var bucket = BucketName();
        var key = $"ai/{folder}/{Guid.NewGuid():N}.png";
        await using var stream = new MemoryStream(pngBytes, writable: false);
        await _objectStorage.PutObjectAsync(
            new ObjectPutRequest
            {
                BucketName = bucket,
                ObjectKey = key,
                Content = stream,
                ObjectSize = pngBytes.Length,
                ContentType = "image/png"
            },
            cancellationToken).ConfigureAwait(false);

        return await ResolvePublicUrlAsync(bucket, key, cancellationToken).ConfigureAwait(false);
    }

    private string BucketName() =>
        string.IsNullOrWhiteSpace(_minio.DefaultBucket) ? OvoStorageBuckets.Media : _minio.DefaultBucket;

    private async Task<string> ResolvePublicUrlAsync(
        string bucket,
        string objectKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_minio.PublicReadBaseUrl))
        {
            var root = _minio.PublicReadBaseUrl!.TrimEnd('/');
            var key = objectKey.TrimStart('/');
            return $"{root}/{key}";
        }

        var seconds = Math.Clamp(_minio.GarmentImagePresignedReadExpirySeconds, 60, 86400);
        return await _objectStorage
            .PresignedGetObjectAsync(bucket, objectKey, TimeSpan.FromSeconds(seconds), cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool TryGetBlockReason(string json, out string? reason)
    {
        reason = null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("promptFeedback", out var pf))
            {
                return false;
            }

            if (pf.TryGetProperty("blockReason", out var br))
            {
                reason = br.GetString();
                return !string.IsNullOrEmpty(reason);
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private static string? ExtractFirstTextPart(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("candidates", out var candArr) || candArr.GetArrayLength() == 0)
        {
            return null;
        }

        var content = candArr[0].GetProperty("content");
        foreach (var part in content.GetProperty("parts").EnumerateArray())
        {
            if (part.TryGetProperty("thought", out var th) && th.ValueKind == JsonValueKind.True)
            {
                continue;
            }

            if (part.TryGetProperty("text", out var textEl))
            {
                return textEl.GetString();
            }
        }

        return null;
    }

    private static byte[]? ExtractFinalImageBytes(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("candidates", out var candArr) || candArr.GetArrayLength() == 0)
        {
            return null;
        }

        byte[]? last = null;
        var content = candArr[0].GetProperty("content");
        foreach (var part in content.GetProperty("parts").EnumerateArray())
        {
            if (part.TryGetProperty("thought", out var th) && th.ValueKind == JsonValueKind.True)
            {
                continue;
            }

            JsonElement inline;
            if (part.TryGetProperty("inlineData", out inline) || part.TryGetProperty("inline_data", out inline))
            {
                if (!inline.TryGetProperty("data", out var dataEl))
                {
                    continue;
                }

                var b64 = dataEl.GetString();
                if (string.IsNullOrEmpty(b64))
                {
                    continue;
                }

                var mime = "image/png";
                if (inline.TryGetProperty("mimeType", out var mt))
                {
                    mime = mt.GetString() ?? mime;
                }
                else if (inline.TryGetProperty("mime_type", out var mt2))
                {
                    mime = mt2.GetString() ?? mime;
                }

                if (mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    last = Convert.FromBase64String(b64);
                }
            }
        }

        return last;
    }

    private static bool TryParseSafeJson(string text, out bool safe)
    {
        safe = false;
        var json = ExtractJsonObject(text);
        if (json == null)
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("safe", out var s) && s.ValueKind == JsonValueKind.True)
            {
                safe = true;
                return true;
            }

            if (doc.RootElement.TryGetProperty("safe", out var f) && f.ValueKind == JsonValueKind.False)
            {
                safe = false;
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private GarmentTagParseResult ParseGarmentTags(string text)
    {
        var fallback = new GarmentTagParseResult
        {
            Category = GarmentCategory.Top,
            SubCategory = "Belirsiz",
            Color = "Belirsiz",
            Pattern = "Düz",
            Seasons = "İlkbahar,Yaz",
            Formality = GarmentFormality.Casual
        };

        var json = ExtractJsonObject(text);
        if (json == null)
        {
            _logger.LogWarning("Kıyafet etiket JSON bulunamadı: {Text}", Truncate(text, 300));
            return fallback;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var cat = GarmentCategory.Top;
            if (root.TryGetProperty("category", out var c) &&
                Enum.TryParse<GarmentCategory>(c.GetString(), ignoreCase: true, out var parsed))
            {
                cat = parsed;
            }

            var form = GarmentFormality.Casual;
            if (root.TryGetProperty("formality", out var fm) &&
                Enum.TryParse<GarmentFormality>(fm.GetString()?.Replace(" ", "", StringComparison.Ordinal) ?? "",
                    ignoreCase: true, out var fp))
            {
                form = fp;
            }

            return new GarmentTagParseResult
            {
                Category = cat,
                SubCategory = root.GetPropertyOrDefault("subCategory", fallback.SubCategory),
                Color = root.GetPropertyOrDefault("color", fallback.Color),
                Pattern = root.GetPropertyOrDefault("pattern", fallback.Pattern),
                Seasons = root.GetPropertyOrDefault("seasons", fallback.Seasons),
                Formality = form
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kıyafet etiket JSON ayrıştırılamadı");
            return fallback;
        }
    }

    private static UserPhotoAnalysisResult? ParseUserPhotoAnalysis(string text)
    {
        var json = ExtractJsonObject(text);
        if (json == null)
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var q = 0.7;
            if (root.TryGetProperty("qualityScore", out var qs) && qs.TryGetDouble(out var d))
            {
                q = Math.Clamp(d, 0, 1);
            }

            var hasFace = root.TryGetProperty("hasFace", out var hf) && hf.ValueKind == JsonValueKind.True;
            var full = root.TryGetProperty("isFullBody", out var fb) && fb.ValueKind == JsonValueKind.True;
            return new UserPhotoAnalysisResult
            {
                QualityScore = q,
                HasFace = hasFace,
                IsFullBody = full
            };
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractJsonObject(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var t = text.Trim();
        var start = t.IndexOf('{');
        var end = t.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return t.Substring(start, end - start + 1);
    }

    private static string? TryExtractOgImage(string html, string pageUrl)
    {
        var patterns = new[]
        {
            """property\s*=\s*["']og:image["']\s+content\s*=\s*["']([^"']+)["']""",
            """content\s*=\s*["']([^"']+)["']\s+property\s*=\s*["']og:image["']""",
            """name\s*=\s*["']twitter:image["']\s+content\s*=\s*["']([^"']+)["']"""
        };

        foreach (var p in patterns)
        {
            var m = Regex.Match(html, p, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (m.Success)
            {
                var raw = m.Groups[1].Value.Trim();
                if (Uri.TryCreate(new Uri(pageUrl), raw, out var abs))
                {
                    return abs.ToString();
                }

                if (Uri.TryCreate(raw, UriKind.Absolute, out var direct))
                {
                    return direct.ToString();
                }
            }
        }

        return null;
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max)
        {
            return s;
        }

        return s[..max] + "…";
    }

    private sealed class GarmentTagParseResult
    {
        public GarmentCategory Category { get; init; }

        public string SubCategory { get; init; } = default!;

        public string Color { get; init; } = default!;

        public string Pattern { get; init; } = default!;

        public string Seasons { get; init; } = default!;

        public GarmentFormality Formality { get; init; }
    }
}

internal static class JsonElementExtensions
{
    public static string GetPropertyOrDefault(this JsonElement root, string name, string fallback)
    {
        return root.TryGetProperty(name, out var el) ? (el.GetString() ?? fallback) : fallback;
    }
}
