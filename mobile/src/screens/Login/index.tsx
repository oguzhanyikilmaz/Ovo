import * as React from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { ActivityIndicator, Alert, Pressable, Text, TextInput, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { getErrorMessage } from '@/services/api-client';
import { loginWithPassword } from '@/services/auth.service';
import { useAuthStore } from '@/store/useAuthStore';
import { styles } from './styles';

interface LoginScreenProps {
  onSuccess: () => void;
}

export function LoginScreen({ onSuccess }: LoginScreenProps) {
  const qc = useQueryClient();
  const setSession = useAuthStore((s) => s.setSession);
  const [user, setUser] = React.useState('');
  const [password, setPassword] = React.useState('');
  const [busy, setBusy] = React.useState(false);

  const submit = async () => {
    if (!user.trim() || !password) {
      Alert.alert(strings.appName, 'Kullanıcı adı ve şifre gerekli.');
      return;
    }
    setBusy(true);
    try {
      await loginWithPassword(user.trim(), password);
      setSession(true, user.trim());
      await qc.invalidateQueries();
      onSuccess();
    } catch (e) {
      Alert.alert(strings.errorGeneric, getErrorMessage(e, strings.errorGeneric));
    } finally {
      setBusy(false);
    }
  };

  return (
    <SafeAreaView style={styles.safe} edges={['top', 'bottom']}>
      <View style={styles.body}>
        <Text style={styles.title}>{strings.login}</Text>
        <Text style={styles.hint}>
          OpenIddict şifre akışı: EXPO_PUBLIC_OIDC_CLIENT_ID (varsayılan OVO_App). Sunucuda bu istemcinin tanımlı
          olduğundan emin olun.
        </Text>
        <TextInput
          style={styles.input}
          placeholder={strings.emailOrUser}
          placeholderTextColor="#71717A"
          autoCapitalize="none"
          autoCorrect={false}
          value={user}
          onChangeText={setUser}
        />
        <TextInput
          style={styles.input}
          placeholder={strings.password}
          placeholderTextColor="#71717A"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />
        <Pressable style={[styles.btn, busy && styles.btnDisabled]} onPress={submit} disabled={busy}>
          {busy ? (
            <ActivityIndicator color={colors.background} />
          ) : (
            <Text style={styles.btnText}>{strings.login}</Text>
          )}
        </Pressable>
      </View>
    </SafeAreaView>
  );
}
