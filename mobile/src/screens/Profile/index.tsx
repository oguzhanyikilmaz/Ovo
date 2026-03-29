import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { ActivityIndicator, Alert, Pressable, Text, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { queryKeys } from '@/hooks/queryKeys';
import type { RootStackParamList } from '@/navigation/types';
import { getErrorMessage } from '@/services/api-client';
import { logout } from '@/services/auth.service';
import { getProfile } from '@/services/profile.service';
import { useAuthStore } from '@/store/useAuthStore';
import { styles } from './styles';

interface ProfileScreenProps {
  onLoggedOut: () => void;
}

export function ProfileScreen({ onLoggedOut }: ProfileScreenProps) {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const qc = useQueryClient();
  const isLoggedIn = useAuthStore((s) => s.isLoggedIn);
  const setSession = useAuthStore((s) => s.setSession);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: queryKeys.profile,
    queryFn: getProfile,
    enabled: isLoggedIn,
  });

  const doLogout = async () => {
    await logout();
    setSession(false, null);
    await qc.clear();
    onLoggedOut();
  };

  const confirmLogout = () => {
    Alert.alert(strings.logout, 'Oturumu kapatmak istiyor musunuz?', [
      { text: 'İptal', style: 'cancel' },
      { text: strings.logout, style: 'destructive', onPress: () => void doLogout() },
    ]);
  };

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
      ) : (
        <View style={styles.body}>
          <Text style={styles.title}>{strings.profile}</Text>
          {data ? (
            <>
              <Text style={styles.line}>Paket: {data.package === 1 ? 'Premium' : 'Ücretsiz'}</Text>
              <Text style={styles.line}>Günlük render: {data.dailyRenderCount}</Text>
              {data.gender ? <Text style={styles.line}>Cinsiyet: {data.gender}</Text> : null}
            </>
          ) : null}
          <Pressable style={[styles.btn, styles.btnDanger]} onPress={confirmLogout}>
            <Text style={styles.btnText}>{strings.logout}</Text>
          </Pressable>
        </View>
      )}
    </SafeAreaView>
  );
}
