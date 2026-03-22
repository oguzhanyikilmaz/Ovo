using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.FileStorage;

/// <summary>
/// Mobil / istemci için presigned URL üretimi. Sunucu tarafı IO için IObjectStorageService kullanılır.
/// </summary>
public interface IFileStorageAppService : IApplicationService
{
    Task<PresignedUrlResultDto> GetPresignedPutUrlAsync(GetPresignedPutUrlInput input);

    Task<PresignedUrlResultDto> GetPresignedGetUrlAsync(GetPresignedGetUrlInput input);
}
