using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace OVO.TryOn;

public interface ITryOnAppService : IApplicationService
{
    Task<UserPhotoDto> AddPhotoAsync(AddTryOnPhotoDto input);

    Task<List<UserPhotoDto>> GetPhotosAsync();

    Task DeletePhotoAsync(Guid id);

    Task<StudioPhotoDto> GetStudioPhotoAsync();

    Task SetStudioPhotoAsync(SetStudioPhotoDto input);

    Task<TryOnRenderResultDto> RenderAsync(TryOnRenderInputDto input);

    Task<TryOnRenderResultDto> GetRenderAsync(string comboHash);
}
