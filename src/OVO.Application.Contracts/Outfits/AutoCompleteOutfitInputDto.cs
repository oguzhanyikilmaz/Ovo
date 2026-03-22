using System;
using System.Collections.Generic;

namespace OVO.Outfits;

public class AutoCompleteOutfitInputDto
{
    public List<Guid> SelectedGarmentIds { get; set; } = new();
}
