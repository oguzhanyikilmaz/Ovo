/**
 * OVO.Application.Contracts + Volo.Abp.Application.Dtos
 * OpenAPI: docs/openapi/swagger.web.v1.full.json (alan adları ASP.NET camelCase)
 */

export interface PagedResultDto<T> {
  totalCount: number;
  items: T[] | null;
}

export type PollFeedFilter = 0 | 1 | 2;

export interface PollListItemDto {
  id: string;
  creatorUserId: string;
  question: string;
  optionCount: number;
  totalVotes: number;
  creationTime: string;
}

export interface VotePollInput {
  optionIndex: number;
}

export type GarmentCategory = 1 | 2 | 3 | 4 | 5;
export type GarmentVisibility = 0 | 1;

export interface GarmentListItemDto {
  id: string;
  category: GarmentCategory;
  subCategory: string;
  color: string;
  cutoutImageUrl: string;
  visibility: GarmentVisibility;
  creationTime: string;
}

export type OutfitVisibility = 0 | 1;

export interface OutfitDto {
  id: string;
  userId: string;
  garmentIds: string[];
  comboHash: string | null;
  harmonyScore: number | null;
  renderUrl: string | null;
  visibility: OutfitVisibility;
  isShared: boolean;
  creationTime: string;
}

export interface OutfitListItemDto {
  id: string;
  garmentIds: string[];
  renderUrl: string | null;
  visibility: OutfitVisibility;
  creationTime: string;
}

export interface WeatherDto {
  city: string;
  temperatureC: number;
  condition: string;
  outfitHint: string;
}

export interface TrendColorDto {
  color: string;
  count: number;
  percent: number;
}

export interface TrendOutfitDto {
  outfitId: string;
  renderUrl: string | null;
  likeScore: number;
}

export type UserPackage = 0 | 1;
export type AccountStatus = 0 | 1 | 2 | 3;

export interface ProfileDto {
  id: string;
  gender: string | null;
  studioPhotoUrl: string | null;
  package: UserPackage;
  dailyRenderCount: number;
  heightCm: number | null;
  weightKg: number | null;
  bodyType: string | null;
  accountStatus: AccountStatus;
  deletionRequestedAt: string | null;
  creationTime: string;
}

export type WishlistContentType = 1 | 2 | 3 | 4;

export interface WishlistItemDto {
  id: string;
  contentType: WishlistContentType;
  contentId: string;
  sourceType: string | null;
  sourceLabel: string | null;
  previewImageUrl: string | null;
  creationTime: string;
}

export interface CreateWishlistItemDto {
  contentType: WishlistContentType;
  contentId: string;
  sourceType?: string | null;
  sourceLabel?: string | null;
  previewImageUrl?: string | null;
}

export interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
}

export interface AbpRemoteServiceError {
  code: string | null;
  message: string | null;
  details: string | null;
  data?: Record<string, unknown>;
  validationErrors?: Array<{
    message: string;
    members: string[];
  }>;
}

export interface AbpErrorResponse {
  error?: AbpRemoteServiceError;
}

/** Try-On — OpenAPI: OVO.TryOn.* */
export interface UserPhotoDto {
  id: string;
  creationTime: string;
  creatorId: string | null;
  photoUrl: string | null;
  qualityScore: number | null;
  hasFace: boolean;
  isFullBody: boolean;
}

export interface StudioPhotoDto {
  studioPhotoUrl: string | null;
}

export interface AddTryOnPhotoDto {
  photoUrl: string;
}

export interface SetStudioPhotoDto {
  studioPhotoUrl?: string | null;
  regenerateFromPhotos: boolean;
  sourcePhotoUrls?: string[] | null;
}

export interface TryOnRenderInputDto {
  garmentIds: string[];
}

export interface TryOnRenderResultDto {
  comboHash: string | null;
  renderUrl: string | null;
}
