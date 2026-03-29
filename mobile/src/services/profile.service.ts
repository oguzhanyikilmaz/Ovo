import { apiClient } from '@/services/api-client';
import type { ProfileDto } from '@/types/ovo-api';

export async function getProfile(): Promise<ProfileDto> {
  const { data } = await apiClient.get<ProfileDto>('/api/app/profile');
  return data;
}
