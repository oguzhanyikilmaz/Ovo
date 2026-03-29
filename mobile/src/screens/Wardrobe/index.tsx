import * as React from 'react';
import { useQuery } from '@tanstack/react-query';
import { ActivityIndicator, Image, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { FlashList } from '@shopify/flash-list';
import { AppHeader } from '@/components/ui/AppHeader';
import { LoginBanner } from '@/components/ui/LoginBanner';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { queryKeys } from '@/hooks/queryKeys';
import { getErrorMessage } from '@/services/api-client';
import { getGarments } from '@/services/wardrobe.service';
import { useRootNav } from '@/hooks/useRootNav';
import { useAuthStore } from '@/store/useAuthStore';
import type { GarmentListItemDto } from '@/types/ovo-api';
import { styles } from './styles';

export function WardrobeScreen() {
  const { openLogin, openWishlist, openProfile } = useRootNav();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);

  const { data, isLoading, isError, error, refetch, isRefetching } = useQuery({
    queryKey: queryKeys.garments(undefined),
    queryFn: () => getGarments({ maxResultCount: 60 }),
    enabled: isLoggedIn,
  });

  const items = data?.items ?? [];

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <AppHeader onWishlist={() => openWishlist?.()} onProfile={() => openProfile?.()} />
      {!isLoggedIn ? (
        <LoginBanner onPressLogin={() => openLogin?.()} />
      ) : isLoading ? (
        <View style={styles.center}>
          <ActivityIndicator color={colors.text} />
        </View>
      ) : isError ? (
        <View style={styles.center}>
          <Text style={styles.muted}>{getErrorMessage(error, strings.errorGeneric)}</Text>
        </View>
      ) : (
        <View style={styles.list}>
          <FlashList
            data={items}
            numColumns={3}
            keyExtractor={(item) => item.id}
            refreshing={isRefetching}
            onRefresh={() => refetch()}
            ListEmptyComponent={
              <View style={styles.center}>
                <Text style={styles.muted}>{strings.wardrobeEmpty}</Text>
              </View>
            }
            renderItem={({ item }) => <GarmentCell item={item} />}
          />
        </View>
      )}
    </SafeAreaView>
  );
}

function GarmentCell({ item }: { item: GarmentListItemDto }) {
  return (
    <View style={styles.cell}>
      <Image source={{ uri: item.cutoutImageUrl }} style={styles.image} resizeMode="cover" />
      <View style={styles.label}>
        <Text style={styles.labelText} numberOfLines={1}>
          {item.subCategory}
        </Text>
      </View>
    </View>
  );
}
