using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.Profile;

public interface IProfileAppService : IApplicationService
{
    Task<ProfileDto> GetAsync();

    Task<ProfileDto> UpdateAsync(UpdateProfileDto input);

    Task FreezeAsync();

    Task RequestDeletionAsync();

    Task CancelDeletionAsync();

    Task<ProfileExportDto> ExportAsync();
}
