import { apiClient } from '@/services/api-client';
import type { WeatherDto } from '@/types/ovo-api';

export async function getWeather(): Promise<WeatherDto> {
  const { data } = await apiClient.get<WeatherDto>('/api/app/weather');
  return data;
}
