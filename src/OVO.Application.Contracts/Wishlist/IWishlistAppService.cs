using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace OVO.Wishlist;

public interface IWishlistAppService : IApplicationService
{
    Task<PagedResultDto<WishlistItemDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<WishlistItemDto> CreateAsync(CreateWishlistItemDto input);

    Task DeleteAsync(Guid id);
}
