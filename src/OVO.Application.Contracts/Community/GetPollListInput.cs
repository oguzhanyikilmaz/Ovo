using Volo.Abp.Application.Dtos;

namespace OVO.Community;

public class GetPollListInput : PagedAndSortedResultRequestDto
{
    public PollFeedFilter Filter { get; set; } = PollFeedFilter.New;
}
