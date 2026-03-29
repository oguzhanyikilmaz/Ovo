import { apiClient } from '@/services/api-client';
import type { PagedResultDto, TrendColorDto, TrendOutfitDto } from '@/types/ovo-api';

export async function getTrendColors(): Promise<TrendColorDto[]> {
  const { data } = await apiClient.get<TrendColorDto[]>('/api/app/trend/colors');
  return data ?? [];
}

export interface GetTrendOutfitsParams {
  sorting?: string;
  skipCount?: number;
  maxResultCount?: number;
}

export async function getTrendOutfits(
  params: GetTrendOutfitsParams = {}
): Promise<PagedResultDto<TrendOutfitDto>> {
  const { data } = await apiClient.get<PagedResultDto<TrendOutfitDto>>('/api/app/trend/outfits', {
    params: {
      Sorting: params.sorting,
      SkipCount: params.skipCount ?? 0,
      MaxResultCount: params.maxResultCount ?? 10,
    },
  });
  return data;
}
