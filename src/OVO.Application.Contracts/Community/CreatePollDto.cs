using System.Collections.Generic;

namespace OVO.Community;

public class CreatePollDto
{
    public string Question { get; set; } = default!;

    public List<PollOptionDto> Options { get; set; } = new();

    public bool Publish { get; set; } = true;
}
