using System;
using Volo.Abp.Application.Dtos;

namespace OVO.Community;

public class PollListItemDto : CreationAuditedEntityDto<Guid>
{
    public Guid CreatorUserId { get; set; }

    public string Question { get; set; } = default!;

    public int OptionCount { get; set; }

    public int TotalVotes { get; set; }
}
