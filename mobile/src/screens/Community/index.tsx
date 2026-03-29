import * as React from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { FlashList } from '@shopify/flash-list';
import { ActivityIndicator, Alert, Pressable, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { AppHeader } from '@/components/ui/AppHeader';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { queryKeys } from '@/hooks/queryKeys';
import { getErrorMessage } from '@/services/api-client';
import { getAccessTokenSync } from '@/services/auth-tokens';
import { getPolls, votePoll } from '@/services/community.service';
import { useAuthStore } from '@/store/useAuthStore';
import type { PollFeedFilter, PollListItemDto } from '@/types/ovo-api';
import { useRootNav } from '@/hooks/useRootNav';
import { styles } from './styles';

const filters: { label: string; value: PollFeedFilter }[] = [
  { label: 'Yeni', value: 0 },
  { label: 'Popüler', value: 1 },
  { label: 'Takip', value: 2 },
];

export function CommunityScreen() {
  const { openLogin, openWishlist, openProfile } = useRootNav();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  const [filter, setFilter] = React.useState<PollFeedFilter>(0);
  const qc = useQueryClient();

  const { data, isLoading, isError, error, refetch, isRefetching } = useQuery({
    queryKey: queryKeys.polls(filter),
    queryFn: () => getPolls({ filter, maxResultCount: 20 }),
  });

  const voteMut = useMutation({
    mutationFn: ({ pollId, optionIndex }: { pollId: string; optionIndex: number }) =>
      votePoll(pollId, { optionIndex }),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['community'] });
    },
    onError: (e) => {
      Alert.alert(strings.errorGeneric, getErrorMessage(e, strings.errorGeneric));
    },
  });

  const items = data?.items ?? [];

  const onVote = (pollId: string, optionIndex: number) => {
    if (!getAccessTokenSync()) {
      Alert.alert(strings.appName, strings.loginRequired);
      openLogin?.();
      return;
    }
    voteMut.mutate({ pollId, optionIndex });
  };

  return (
    <SafeAreaView style={styles.safe} edges={['top']}>
      <AppHeader onWishlist={() => openWishlist?.()} onProfile={() => openProfile?.()} />
      <View style={styles.filters}>
        {filters.map((f) => (
          <Pressable
            key={f.value}
            style={[styles.chip, filter === f.value && styles.chipActive]}
            onPress={() => setFilter(f.value)}
          >
            <Text style={[styles.chipText, filter === f.value && styles.chipTextActive]}>{f.label}</Text>
          </Pressable>
        ))}
      </View>
      {isLoading ? (
        <View style={styles.center}>
          <ActivityIndicator color={colors.text} />
        </View>
      ) : isError ? (
        <View style={styles.center}>
          <Text style={styles.muted}>{getErrorMessage(error, strings.errorGeneric)}</Text>
          <Pressable onPress={() => refetch()} style={{ marginTop: 16 }}>
            <Text style={{ color: colors.text }}>{strings.retry}</Text>
          </Pressable>
        </View>
      ) : (
        <View style={styles.body}>
          <FlashList
            data={items}
            keyExtractor={(item) => item.id}
            refreshing={isRefetching}
            onRefresh={() => refetch()}
            ListEmptyComponent={
              <View style={styles.center}>
                <Text style={styles.muted}>{strings.pollsEmpty}</Text>
              </View>
            }
            renderItem={({ item }) => (
              <PollCard item={item} onVote={onVote} voting={voteMut.isPending} isLoggedIn={isLoggedIn} />
            )}
          />
        </View>
      )}
    </SafeAreaView>
  );
}

function PollCard({
  item,
  onVote,
  voting,
  isLoggedIn,
}: {
  item: PollListItemDto;
  onVote: (id: string, idx: number) => void;
  voting: boolean;
  isLoggedIn: boolean;
}) {
  const optionButtons = Math.min(Math.max(item.optionCount, 1), 3);
  const labels = ['A', 'B', 'C'];
  return (
    <View style={styles.card}>
      <Text style={styles.question}>{item.question}</Text>
      <Text style={styles.meta}>
        {item.totalVotes} oy · {new Date(item.creationTime).toLocaleDateString('tr-TR')}
      </Text>
      <View style={styles.voteRow}>
        {Array.from({ length: optionButtons }).map((_, idx) => (
          <Pressable
            key={idx}
            style={[styles.voteBtn, voting && { opacity: 0.5 }]}
            disabled={voting || !isLoggedIn}
            onPress={() => onVote(item.id, idx)}
          >
            <Text style={styles.voteBtnText}>
              {strings.vote} {labels[idx] ?? idx + 1}
            </Text>
          </Pressable>
        ))}
      </View>
    </View>
  );
}
