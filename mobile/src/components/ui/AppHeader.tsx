import { Pressable, StyleSheet, Text, View } from 'react-native';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';

interface AppHeaderProps {
  onWishlist?: () => void;
  onProfile?: () => void;
}

export function AppHeader({ onWishlist, onProfile }: AppHeaderProps) {
  return (
    <View style={styles.row}>
      <Text style={styles.logo}>{strings.appName}</Text>
      <View style={styles.actions}>
        {onWishlist ? (
          <Pressable onPress={onWishlist} hitSlop={8}>
            <Text style={styles.icon}>♡</Text>
          </Pressable>
        ) : null}
        {onProfile ? (
          <Pressable onPress={onProfile} hitSlop={8}>
            <Text style={styles.icon}>◉</Text>
          </Pressable>
        ) : null}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  row: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 16,
    paddingVertical: 12,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: colors.border,
    backgroundColor: colors.background,
  },
  logo: {
    color: colors.text,
    fontSize: 20,
    fontWeight: '700',
    letterSpacing: 2,
  },
  actions: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 20,
  },
  icon: {
    color: colors.text,
    fontSize: 22,
  },
});
