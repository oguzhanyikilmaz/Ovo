using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace OVO.Community;

public interface ICommunityAppService : IApplicationService
{
    Task<PagedResultDto<PollListItemDto>> GetPollsAsync(GetPollListInput input);

    Task<PollDto> CreatePollAsync(CreatePollDto input);

    Task<PollDto> VoteAsync(Guid pollId, VotePollInput input);
}
