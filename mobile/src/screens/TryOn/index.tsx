import * as React from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ActivityIndicator,
  Image,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { AppHeader } from '@/components/ui/AppHeader';
import { LoginBanner } from '@/components/ui/LoginBanner';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { queryKeys } from '@/hooks/queryKeys';
import { getErrorMessage } from '@/services/api-client';
import {
  addTryOnPhoto,
  deleteTryOnPhoto,
  getStudioPhoto,
  getTryOnPhotos,
  renderTryOn,
  setStudioPhoto,
} from '@/services/tryon.service';
import { getGarments } from '@/services/wardrobe.service';
import { useRootNav } from '@/hooks/useRootNav';
import { useAuthStore } from '@/store/useAuthStore';
import type { GarmentListItemDto } from '@/types/ovo-api';
import { styles } from './styles';

const MIN_GARMENTS = 3;
const MIN_STUDIO_PHOTOS = 3;

export function TryOnScreen() {
  const { openLogin, openWishlist, openProfile } = useRootNav();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  const queryClient = useQueryClient();
  const [photoUrlInput, setPhotoUrlInput] = React.useState('');
  const [selectedIds, setSelectedIds] = React.useState<Set<string>>(() => new Set());
  const [lastRender, setLastRender] = React.useState<{ url: string; hash: string } | null>(null);
  const [actionError, setActionError] = React.useState<string | null>(null);

  const studioQuery = useQuery({
    queryKey: queryKeys.tryOnStudio,
    queryFn: getStudioPhoto,
    enabled: isLoggedIn,
  });

  const photosQuery = useQuery({
    queryKey: queryKeys.tryOnPhotos,
    queryFn: getTryOnPhotos,
    enabled: isLoggedIn,
  });

  const garmentsQuery = useQuery({
    queryKey: queryKeys.garments(undefined),
    queryFn: () => getGarments({ maxResultCount: 60 }),
    enabled: isLoggedIn,
  });

  const addPhotoMutation = useMutation({
    mutationFn: addTryOnPhoto,
    onSuccess: async () => {
      setPhotoUrlInput('');
      setActionError(null);
      await queryClient.invalidateQueries({ queryKey: queryKeys.tryOnPhotos });
    },
    onError: (e) => setActionError(getErrorMessage(e, strings.errorGeneric)),
  });

  const deletePhotoMutation = useMutation({
    mutationFn: deleteTryOnPhoto,
    onSuccess: async () => {
      setActionError(null);
      await queryClient.invalidateQueries({ queryKey: queryKeys.tryOnPhotos });
    },
    onError: (e) => setActionError(getErrorMessage(e, strings.errorGeneric)),
  });

  const studioMutation = useMutation({
    mutationFn: setStudioPhoto,
    onSuccess: async () => {
      setActionError(null);
      await queryClient.invalidateQueries({ queryKey: queryKeys.tryOnStudio });
    },
    onError: (e) => setActionError(getErrorMessage(e, strings.errorGeneric)),
  });

  const renderMutation = useMutation({
    mutationFn: renderTryOn,
    onSuccess: (data) => {
      setActionError(null);
      const url = data.renderUrl?.trim() ?? '';
      const hash = data.comboHash?.trim() ?? '';
      if (url && hash) {
        setLastRender({ url, hash });
      }
    },
    onError: (e) => setActionError(getErrorMessage(e, strings.errorGeneric)),
  });

  const garments = garmentsQuery.data?.items ?? [];
  const photos = photosQuery.data ?? [];
  const studioUrl = studioQuery.data?.studioPhotoUrl?.trim() ?? '';

  const toggleGarment = (id: string) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const onRegenerateStudio = () => {
    const urls = photos
      .map((p) => p.photoUrl?.trim())
      .filter((u): u is string => Boolean(u))
      .slice(0, 15);
    if (urls.length < MIN_STUDIO_PHOTOS) {
      setActionError(strings.tryOnRegenerateNeedPhotos);
      return;
    }
    studioMutation.mutate({
      regenerateFromPhotos: true,
      sourcePhotoUrls: urls,
    });
  };

  const onAddPhoto = () => {
    const url = photoUrlInput.trim();
    if (!url) {
      setActionError(strings.tryOnPhotoUrlRequired);
      return;
    }
    addPhotoMutation.mutate({ photoUrl: url });
  };

  const onRender = () => {
    const ids = [...selectedIds];
    if (ids.length < MIN_GARMENTS) {
      setActionError(strings.tryOnNeedThreeGarments);
      return;
    }
    renderMutation.mutate({ garmentIds: ids });
  };

  const busy =
    addPhotoMutation.isPending ||
    deletePhotoMutation.isPending ||
    studioMutation.isPending ||
    renderMutation.isPending;

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <AppHeader onWishlist={() => openWishlist?.()} onProfile={() => openProfile?.()} />
      {!isLoggedIn ? (
        <LoginBanner onPressLogin={() => openLogin?.()} />
      ) : studioQuery.isLoading || photosQuery.isLoading || garmentsQuery.isLoading ? (
        <View style={styles.center}>
          <ActivityIndicator color={colors.text} />
        </View>
      ) : studioQuery.isError || photosQuery.isError || garmentsQuery.isError ? (
        <View style={styles.center}>
          <Text style={styles.muted}>
            {getErrorMessage(
              studioQuery.error ?? photosQuery.error ?? garmentsQuery.error,
              strings.errorGeneric,
            )}
          </Text>
        </View>
      ) : (
        <ScrollView
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
          showsVerticalScrollIndicator={false}
        >
          <Text style={styles.sectionTitle}>{strings.tryOn}</Text>
          <Text style={styles.muted}>{strings.tryOnRenderHint}</Text>

          {actionError ? <Text style={styles.errorText}>{actionError}</Text> : null}

          <View>
            <Text style={styles.sectionTitle}>{strings.tryOnStudioTitle}</Text>
            {studioUrl ? (
              <Image source={{ uri: studioUrl }} style={styles.studioImage} resizeMode="cover" />
            ) : (
              <Text style={styles.muted}>{strings.tryOnStudioMissing}</Text>
            )}
            <Pressable
              style={[styles.btn, styles.btnSecondary, busy && { opacity: 0.6 }]}
              onPress={onRegenerateStudio}
              disabled={busy}
            >
              <Text style={styles.btnSecondaryText}>{strings.tryOnRegenerateStudio}</Text>
            </Pressable>
          </View>

          <View>
            <Text style={styles.sectionTitle}>{strings.tryOnPhotosTitle}</Text>
            <TextInput
              style={styles.input}
              placeholder={strings.tryOnPhotoUrlPlaceholder}
              placeholderTextColor={colors.textMuted}
              value={photoUrlInput}
              onChangeText={setPhotoUrlInput}
              autoCapitalize="none"
              autoCorrect={false}
              keyboardType="url"
            />
            <Pressable style={[styles.btn, busy && { opacity: 0.6 }]} onPress={onAddPhoto} disabled={busy}>
              <Text style={styles.btnText}>{strings.tryOnAddPhotoUrl}</Text>
            </Pressable>
            {photos.map((p) => (
              <View key={p.id} style={styles.photoRow}>
                <Text style={styles.muted} numberOfLines={1}>
                  {p.photoUrl ?? p.id}
                </Text>
                <Pressable
                  onPress={() => deletePhotoMutation.mutate(p.id)}
                  disabled={deletePhotoMutation.isPending}
                >
                  <Text style={[styles.muted, { color: colors.danger }]}>Sil</Text>
                </Pressable>
              </View>
            ))}
          </View>

          <View>
            <Text style={styles.sectionTitle}>{strings.tryOnGarmentsTitle}</Text>
            {garments.length === 0 ? (
              <Text style={styles.muted}>{strings.wardrobeEmpty}</Text>
            ) : (
              <View style={styles.row}>
                {garments.map((g) => (
                  <GarmentSelectChip
                    key={g.id}
                    item={g}
                    selected={selectedIds.has(g.id)}
                    onToggle={() => toggleGarment(g.id)}
                  />
                ))}
              </View>
            )}
            <Pressable style={[styles.btn, busy && { opacity: 0.6 }]} onPress={onRender} disabled={busy}>
              <Text style={styles.btnText}>{strings.tryOnRender}</Text>
            </Pressable>
          </View>

          {lastRender ? (
            <View>
              <Text style={styles.sectionTitle}>{strings.tryOnResultTitle}</Text>
              <Text style={styles.muted} numberOfLines={1}>
                {lastRender.hash}
              </Text>
              <Image source={{ uri: lastRender.url }} style={styles.renderImage} resizeMode="cover" />
            </View>
          ) : null}
        </ScrollView>
      )}
    </SafeAreaView>
  );
}

function GarmentSelectChip({
  item,
  selected,
  onToggle,
}: {
  item: GarmentListItemDto;
  selected: boolean;
  onToggle: () => void;
}) {
  return (
    <Pressable
      onPress={onToggle}
      style={[styles.garmentChip, selected && styles.garmentChipSelected]}
    >
      <Image source={{ uri: item.cutoutImageUrl }} style={styles.garmentThumb} resizeMode="cover" />
    </Pressable>
  );
}
