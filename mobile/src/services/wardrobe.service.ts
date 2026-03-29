import { apiClient } from '@/services/api-client';
import type { GarmentCategory, GarmentListItemDto, PagedResultDto } from '@/types/ovo-api';

export interface GetWardrobeParams {
  category?: GarmentCategory;
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export async function getGarments(params: GetWardrobeParams = {}): Promise<PagedResultDto<GarmentListItemDto>> {
  const { data } = await apiClient.get<PagedResultDto<GarmentListItemDto>>('/api/app/wardrobe', {
    params: {
      Category: params.category,
      Sorting: params.sorting,
      SkipCount: params.skipCount ?? 0,
      MaxResultCount: params.maxResultCount ?? 30,
    },
  });
  return data;
}
