using Volo.Abp.Application.Dtos;

namespace OVO.Wardrobe;

public class GetGarmentListInput : PagedAndSortedResultRequestDto
{
    public GarmentCategory? Category { get; set; }
}
