using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OVO.Permissions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace OVO.Wishlist;

[Authorize(OVOPermissions.Wishlist.Default)]
public class WishlistAppService : OVOAppService, IWishlistAppService
{
    private readonly IWishlistItemRepository _wishlistItemRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public WishlistAppService(
        IWishlistItemRepository wishlistItemRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _wishlistItemRepository = wishlistItemRepository;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<PagedResultDto<WishlistItemDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var userId = CurrentUser.GetId();
        var q = (await _wishlistItemRepository.GetQueryableAsync()).Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreationTime);
        var total = await _asyncExecuter.CountAsync(q);
        var items = await _asyncExecuter.ToListAsync(q.PageBy(input.SkipCount, input.MaxResultCount));
        return new PagedResultDto<WishlistItemDto>(
            total,
            items.Select(w => ObjectMapper.Map<WishlistItem, WishlistItemDto>(w)).ToList());
    }

    public virtual async Task<WishlistItemDto> CreateAsync(CreateWishlistItemDto input)
    {
        var userId = CurrentUser.GetId();
        var q = await _wishlistItemRepository.GetQueryableAsync();
        var exists = await _asyncExecuter.AnyAsync(
            q.Where(w => w.UserId == userId && w.ContentType == input.ContentType && w.ContentId == input.ContentId));
        if (exists)
        {
            throw new UserFriendlyException("Bu içerik zaten favorilerde.");
        }

        var item = new WishlistItem(
            GuidGenerator.Create(),
            userId,
            input.ContentType,
            input.ContentId,
            CurrentTenant.Id,
            input.SourceType,
            input.SourceLabel,
            input.PreviewImageUrl);

        await _wishlistItemRepository.InsertAsync(item, autoSave: true);
        return ObjectMapper.Map<WishlistItem, WishlistItemDto>(item);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var item = await _wishlistItemRepository.GetAsync(id);
        if (item.UserId != CurrentUser.GetId())
        {
            throw new UserFriendlyException("Bu kayda erişemezsiniz.");
        }

        await _wishlistItemRepository.DeleteAsync(item, autoSave: true);
    }
}
