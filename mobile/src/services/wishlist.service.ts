import { apiClient } from '@/services/api-client';
import type { CreateWishlistItemDto, PagedResultDto, WishlistItemDto } from '@/types/ovo-api';

export interface GetWishlistParams {
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export async function getWishlist(params: GetWishlistParams = {}): Promise<PagedResultDto<WishlistItemDto>> {
  const { data } = await apiClient.get<PagedResultDto<WishlistItemDto>>('/api/app/wishlist', {
    params: {
      Sorting: params.sorting,
      SkipCount: params.skipCount ?? 0,
      MaxResultCount: params.maxResultCount ?? 30,
    },
  });
  return data;
}

export async function addWishlistItem(body: CreateWishlistItemDto): Promise<WishlistItemDto> {
  const { data } = await apiClient.post<WishlistItemDto>('/api/app/wishlist', body);
  return data;
}

export async function removeWishlistItem(id: string): Promise<void> {
  await apiClient.delete(`/api/app/wishlist/${id}`);
}
