import { StyleSheet } from 'react-native';
import { colors } from '@/constants/colors';

export const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: colors.background },
  body: { padding: 24, gap: 12 },
  title: { color: colors.text, fontSize: 20, fontWeight: '700' },
  line: { color: colors.textMuted, fontSize: 14 },
  btn: {
    marginTop: 24,
    paddingVertical: 14,
    borderRadius: 10,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.border,
    alignItems: 'center',
  },
  btnDanger: { borderColor: colors.danger },
  btnText: { color: colors.text, fontWeight: '600' },
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  muted: { color: colors.textMuted },
});
