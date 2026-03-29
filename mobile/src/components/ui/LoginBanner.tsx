import { Pressable, StyleSheet, Text, View } from 'react-native';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';

interface LoginBannerProps {
  onPressLogin: () => void;
}

export function LoginBanner({ onPressLogin }: LoginBannerProps) {
  return (
    <View style={styles.wrap}>
      <Text style={styles.text}>{strings.loginRequired}</Text>
      <Pressable style={styles.btn} onPress={onPressLogin}>
        <Text style={styles.btnText}>{strings.login}</Text>
      </Pressable>
    </View>
  );
}

const styles = StyleSheet.create({
  wrap: {
    marginHorizontal: 16,
    marginVertical: 12,
    padding: 16,
    borderRadius: 12,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    gap: 12,
  },
  text: {
    color: colors.textMuted,
    fontSize: 14,
    lineHeight: 20,
  },
  btn: {
    alignSelf: 'flex-start',
    backgroundColor: colors.accent,
    paddingHorizontal: 16,
    paddingVertical: 10,
    borderRadius: 8,
  },
  btnText: {
    color: colors.background,
    fontWeight: '600',
    fontSize: 14,
  },
});
