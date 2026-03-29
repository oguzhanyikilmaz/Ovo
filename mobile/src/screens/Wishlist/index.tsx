import { useQuery } from '@tanstack/react-query';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { ActivityIndicator, Image, Pressable, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { queryKeys } from '@/hooks/queryKeys';
import type { RootStackParamList } from '@/navigation/types';
import { getErrorMessage } from '@/services/api-client';
import { getWishlist } from '@/services/wishlist.service';
import { useAuthStore } from '@/store/useAuthStore';
import { styles } from './styles';

export function WishlistScreen() {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: queryKeys.wishlist,
    queryFn: () => getWishlist({ maxResultCount: 50 }),
    enabled: isLoggedIn,
  });

  const items = data?.items ?? [];

  if (!isLoggedIn) {
    return (
      <SafeAreaView style={styles.safe} edges={['bottom']}>
        <View style={styles.center}>
          <Text style={styles.muted}>{strings.loginRequired}</Text>
          <Pressable
            style={{ marginTop: 16, padding: 12, backgroundColor: colors.accent, borderRadius: 8 }}
            onPress={() => navigation.navigate('Login')}
          >
            <Text style={{ color: colors.background, fontWeight: '600' }}>{strings.login}</Text>
          </Pressable>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safe} edges={['bottom']}>
      {isLoading ? (
        <View style={styles.center}>
          <ActivityIndicator color={colors.text} />
        </View>
      ) : isError ? (
        <View style={styles.center}>
          <Text style={styles.muted}>{getErrorMessage(error, strings.errorGeneric)}</Text>
        </View>
      ) : items.length === 0 ? (
        <View style={styles.center}>
          <Text style={styles.muted}>Favori yok.</Text>
        </View>
      ) : (
        <View>
          {items.map((w) => (
            <View key={w.id} style={styles.row}>
              {w.previewImageUrl ? (
                <Image source={{ uri: w.previewImageUrl }} style={styles.thumb} />
              ) : (
                <View style={styles.thumb} />
              )}
              <View style={{ flex: 1 }}>
                <Text style={styles.title}>{w.sourceLabel ?? String(w.contentType)}</Text>
                <Text style={styles.sub} numberOfLines={2}>
                  {w.contentId}
                </Text>
              </View>
            </View>
          ))}
        </View>
      )}
    </SafeAreaView>
  );
}
