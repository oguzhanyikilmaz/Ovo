import { apiClient } from '@/services/api-client';
import type {
  AddTryOnPhotoDto,
  SetStudioPhotoDto,
  StudioPhotoDto,
  TryOnRenderInputDto,
  TryOnRenderResultDto,
  UserPhotoDto,
} from '@/types/ovo-api';

export async function getTryOnPhotos(): Promise<UserPhotoDto[]> {
  const { data } = await apiClient.get<UserPhotoDto[]>('/api/app/try-on/photos');
  return data ?? [];
}

export async function addTryOnPhoto(input: AddTryOnPhotoDto): Promise<UserPhotoDto> {
  const { data } = await apiClient.post<UserPhotoDto>('/api/app/try-on/photo', input);
  if (!data) {
    throw new Error('Try-on foto yanıtı boş.');
  }
  return data;
}

export async function deleteTryOnPhoto(id: string): Promise<void> {
  await apiClient.delete(`/api/app/try-on/${id}/photo`);
}

export async function getStudioPhoto(): Promise<StudioPhotoDto> {
  const { data } = await apiClient.get<StudioPhotoDto>('/api/app/try-on/studio-photo');
  if (!data) {
    throw new Error('Stüdyo foto yanıtı boş.');
  }
  return data;
}

export async function setStudioPhoto(input: SetStudioPhotoDto): Promise<void> {
  await apiClient.post('/api/app/try-on/set-studio-photo', input);
}

export async function renderTryOn(input: TryOnRenderInputDto): Promise<TryOnRenderResultDto> {
  const { data } = await apiClient.post<TryOnRenderResultDto>('/api/app/try-on/render', input);
  if (!data) {
    throw new Error('Render yanıtı boş.');
  }
  return data;
}

export async function getRenderByComboHash(comboHash: string): Promise<TryOnRenderResultDto> {
  const { data } = await apiClient.get<TryOnRenderResultDto>('/api/app/try-on/render', {
    params: { comboHash },
  });
  if (!data) {
    throw new Error('Önbellek render yanıtı boş.');
  }
  return data;
}
