export const queryKeys = {
  polls: (filter: number) => ['community', 'polls', filter] as const,
  garments: (category?: number) => ['wardrobe', 'garments', category ?? 'all'] as const,
  dailyOutfit: ['outfit', 'daily'] as const,
  outfitList: ['outfit', 'list'] as const,
  weather: ['weather'] as const,
  trendColors: ['trend', 'colors'] as const,
  trendOutfits: ['trend', 'outfits'] as const,
  profile: ['profile'] as const,
  wishlist: ['wishlist'] as const,
  tryOnPhotos: ['try-on', 'photos'] as const,
  tryOnStudio: ['try-on', 'studio'] as const,
};
