import * as React from 'react';
import { ActivityIndicator, View } from 'react-native';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { StatusBar } from 'expo-status-bar';
import { colors } from '@/constants/colors';
import { loadTokensIntoMemory, getAccessTokenSync } from '@/services/auth-tokens';
import { useAuthStore } from '@/store/useAuthStore';
import { RootNavigator } from '@/navigation/RootNavigator';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 60 * 1000,
      retry: 1,
    },
  },
});

export default function App() {
  const [ready, setReady] = React.useState(false);
  const setHydrated = useAuthStore((s) => s.setHydrated);
  const setSession = useAuthStore((s) => s.setSession);

  React.useEffect(() => {
    void (async () => {
      await loadTokensIntoMemory();
      const loggedIn = getAccessTokenSync() != null;
      setSession(loggedIn, null);
      setHydrated(true);
      setReady(true);
    })();
  }, [setHydrated, setSession]);

  if (!ready) {
    return (
      <View style={{ flex: 1, backgroundColor: colors.background, justifyContent: 'center', alignItems: 'center' }}>
        <ActivityIndicator color={colors.text} size="large" />
        <StatusBar style="light" />
      </View>
    );
  }

  return (
    <QueryClientProvider client={queryClient}>
      <StatusBar style="light" />
      <RootNavigator />
    </QueryClientProvider>
  );
}
