using System;
using System.Collections.Generic;

namespace OVO.TryOn;

public class TryOnRenderInputDto
{
    public List<Guid> GarmentIds { get; set; } = new();
}
