using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace OVO.Community;

public class PollDto : FullAuditedEntityDto<Guid>
{
    public Guid CreatorUserId { get; set; }

    public string Question { get; set; } = default!;

    public List<PollOptionDto> Options { get; set; } = new();

    public List<int> OptionVoteCounts { get; set; } = new();

    public int TotalVotes { get; set; }

    public bool IsPublished { get; set; }
}
