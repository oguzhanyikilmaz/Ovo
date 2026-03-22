using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OVO.Permissions;
using OVO.Wardrobe;
using Volo.Abp;

namespace OVO.Controllers;

/// <summary>
/// Multipart kıyafet görseli — iş mantığı <see cref="IWardrobeAppService"/> içinde.
/// </summary>
[Area("app")]
[Route("api/app/wardrobe")]
[Authorize(OVOPermissions.Wardrobe.Default)]
public class WardrobeUploadController : OVOController
{
    private readonly IWardrobeAppService _wardrobeAppService;

    public WardrobeUploadController(IWardrobeAppService wardrobeAppService)
    {
        _wardrobeAppService = wardrobeAppService;
    }

    /// <summary>
    /// Form: <c>file</c> (görsel) + <c>metadataJson</c> (<see cref="CreateGarmentMultipartMetadataDto"/> JSON).
    /// </summary>
    [HttpPost("garment-image-multipart")]
    [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
    [RequestSizeLimit(21 * 1024 * 1024)]
    [ProducesResponseType(typeof(GarmentDto), StatusCodes.Status200OK)]
    public virtual async Task<GarmentDto> UploadGarmentImageAsync(
        IFormFile file,
        [FromForm] string metadataJson,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new UserFriendlyException("Dosya gerekli.");
        }

        if (file.Length > WardrobeUploadLimits.MaxImageBytes)
        {
            throw new UserFriendlyException("Dosya çok büyük (en fazla 20 MB).");
        }

        CreateGarmentMultipartMetadataDto? metadata;
        try
        {
            metadata = JsonSerializer.Deserialize<CreateGarmentMultipartMetadataDto>(
                metadataJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("metadataJson geçersiz JSON.");
        }

        if (metadata == null)
        {
            throw new UserFriendlyException("metadataJson gerekli.");
        }

        await using var stream = file.OpenReadStream();
        return await _wardrobeAppService.CreateFromUploadStreamAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            metadata,
            cancellationToken);
    }
}
