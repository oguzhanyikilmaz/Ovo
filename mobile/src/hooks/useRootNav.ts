import { useNavigation } from '@react-navigation/native';
import type { NativeStackNavigationProp } from '@react-navigation/native-stack';
import type { RootStackParamList } from '@/navigation/types';

/**
 * Tab içinden kök stack ekranlarına (Login, Wishlist, Profile) gitmek için.
 */
export function useRootNav() {
  const nav = useNavigation();
  const parent = nav.getParent() as NativeStackNavigationProp<RootStackParamList> | undefined;

  return {
    openLogin: () => parent?.navigate('Login'),
    openWishlist: () => parent?.navigate('Wishlist'),
    openProfile: () => parent?.navigate('Profile'),
  };
}
