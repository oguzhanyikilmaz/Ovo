import * as React from 'react';
import { useQuery } from '@tanstack/react-query';
import { ActivityIndicator, Image, Pressable, ScrollView, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { AppHeader } from '@/components/ui/AppHeader';
import { LoginBanner } from '@/components/ui/LoginBanner';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { queryKeys } from '@/hooks/queryKeys';
import { getErrorMessage } from '@/services/api-client';
import { getDailyOutfit } from '@/services/outfit.service';
import { getTrendColors } from '@/services/trend.service';
import { getWeather } from '@/services/weather.service';
import { useRootNav } from '@/hooks/useRootNav';
import { useAuthStore } from '@/store/useAuthStore';
import { styles } from './styles';

export function OutfitsScreen() {
  const { openLogin, openWishlist, openProfile } = useRootNav();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);

  const weatherQ = useQuery({
    queryKey: queryKeys.weather,
    queryFn: getWeather,
    enabled: isLoggedIn,
  });

  const dailyQ = useQuery({
    queryKey: queryKeys.dailyOutfit,
    queryFn: getDailyOutfit,
    enabled: isLoggedIn,
  });

  const colorsQ = useQuery({
    queryKey: queryKeys.trendColors,
    queryFn: getTrendColors,
    enabled: isLoggedIn,
  });

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <AppHeader onWishlist={() => openWishlist?.()} onProfile={() => openProfile?.()} />
      <ScrollView style={styles.scroll} contentContainerStyle={{ paddingBottom: 32 }}>
        {!isLoggedIn ? (
          <LoginBanner onPressLogin={() => openLogin?.()} />
        ) : (
          <>
            {weatherQ.isLoading ? (
              <View style={styles.center}>
                <ActivityIndicator color={colors.text} />
              </View>
            ) : weatherQ.isError ? (
              <View style={styles.weather}>
                <Text style={styles.weatherSub}>{strings.weatherLoadError}</Text>
                <Pressable onPress={() => weatherQ.refetch()}>
                  <Text style={{ color: colors.text, marginTop: 8 }}>{strings.retry}</Text>
                </Pressable>
              </View>
            ) : weatherQ.data ? (
              <View style={styles.weather}>
                <Text style={styles.weatherTitle}>
                  {weatherQ.data.city} · {weatherQ.data.temperatureC.toFixed(0)}°C · {weatherQ.data.condition}
                </Text>
                <Text style={styles.weatherSub}>{weatherQ.data.outfitHint}</Text>
              </View>
            ) : null}

            <Text style={styles.sectionTitle}>Günün kombini</Text>
            {dailyQ.isLoading ? (
              <View style={[styles.outfitCard, { alignItems: 'center' }]}>
                <ActivityIndicator color={colors.text} />
              </View>
            ) : dailyQ.isError ? (
              <View style={styles.outfitCard}>
                <Text style={styles.muted}>{getErrorMessage(dailyQ.error, strings.outfitEmpty)}</Text>
              </View>
            ) : dailyQ.data ? (
              <View style={styles.outfitCard}>
                {dailyQ.data.renderUrl ? (
                  <Image
                    source={{ uri: dailyQ.data.renderUrl }}
                    style={{ width: '100%', height: 220, borderRadius: 8 }}
                    resizeMode="cover"
                  />
                ) : (
                  <Text style={styles.weatherTitle}>Kombin hazır</Text>
                )}
                <Text style={styles.outfitMeta}>
                  {dailyQ.data.garmentIds.length} parça · uyum:{' '}
                  {dailyQ.data.harmonyScore != null ? dailyQ.data.harmonyScore.toFixed(2) : '—'}
                </Text>
              </View>
            ) : null}

            <Text style={styles.sectionTitle}>Trend renkler</Text>
            {colorsQ.isLoading ? (
              <ActivityIndicator color={colors.text} style={{ marginLeft: 16 }} />
            ) : (
              <View style={{ paddingHorizontal: 16, gap: 8 }}>
                {(colorsQ.data ?? []).map((c) => (
                  <View key={c.color} style={{ flexDirection: 'row', alignItems: 'center', gap: 12 }}>
                    <View style={styles.colorDot}>
                      <Text style={styles.colorLabel}>{c.percent.toFixed(0)}%</Text>
                    </View>
                    <Text style={styles.weatherSub}>
                      {c.color} · {c.count} adet
                    </Text>
                  </View>
                ))}
              </View>
            )}
          </>
        )}
      </ScrollView>
    </SafeAreaView>
  );
}
