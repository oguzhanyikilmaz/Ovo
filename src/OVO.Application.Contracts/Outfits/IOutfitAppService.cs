using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace OVO.Outfits;

public interface IOutfitAppService : IApplicationService
{
    Task<OutfitDto> GetDailyAsync();

    Task<List<OutfitSuggestionDto>> GetSuggestionsAsync();

    Task<PagedResultDto<OutfitListItemDto>> GetListAsync(GetOutfitListInput input);

    Task<OutfitDto> GetAsync(Guid id);

    Task<OutfitDto> CreateAsync(CreateOutfitDto input);

    Task<OutfitDto> UpdateAsync(Guid id, UpdateOutfitDto input);

    Task DeleteAsync(Guid id);

    Task<AutoCompleteOutfitResultDto> AutoCompleteAsync(AutoCompleteOutfitInputDto input);

    Task ShareAsync(Guid id);
}
