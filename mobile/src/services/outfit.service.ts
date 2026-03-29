import { apiClient } from '@/services/api-client';
import type { OutfitDto, OutfitListItemDto, PagedResultDto } from '@/types/ovo-api';

export async function getDailyOutfit(): Promise<OutfitDto> {
  const { data } = await apiClient.get<OutfitDto>('/api/app/outfit/daily');
  return data;
}

export interface GetOutfitsParams {
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export async function getOutfitList(
  params: GetOutfitsParams = {}
): Promise<PagedResultDto<OutfitListItemDto>> {
  const { data } = await apiClient.get<PagedResultDto<OutfitListItemDto>>('/api/app/outfit', {
    params: {
      Sorting: params.sorting,
      SkipCount: params.skipCount ?? 0,
      MaxResultCount: params.maxResultCount ?? 20,
    },
  });
  return data;
}
