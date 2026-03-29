import * as React from 'react';
import { NavigationContainer, DefaultTheme } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { Text } from 'react-native';
import { colors } from '@/constants/colors';
import { strings } from '@/constants/strings';
import { CommunityScreen } from '@/screens/Community';
import { LoginScreen } from '@/screens/Login';
import { OutfitsScreen } from '@/screens/Outfits';
import { ProfileScreen } from '@/screens/Profile';
import { TryOnScreen } from '@/screens/TryOn';
import { WardrobeScreen } from '@/screens/Wardrobe';
import { WishlistScreen } from '@/screens/Wishlist';
import type { MainTabParamList, RootStackParamList } from '@/navigation/types';

const Stack = createNativeStackNavigator<RootStackParamList>();
const Tab = createBottomTabNavigator<MainTabParamList>();

const navTheme = {
  ...DefaultTheme,
  colors: {
    ...DefaultTheme.colors,
    background: colors.background,
    card: colors.background,
    text: colors.text,
    border: colors.border,
    primary: colors.accent,
  },
};

function MainTabs() {
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarStyle: {
          backgroundColor: colors.background,
          borderTopColor: colors.border,
        },
        tabBarActiveTintColor: colors.text,
        tabBarInactiveTintColor: colors.textMuted,
      }}
    >
      <Tab.Screen
        name="OutfitsTab"
        component={OutfitsScreen}
        options={{ title: strings.outfits, tabBarIcon: () => <TabIcon label="⌁" /> }}
      />
      <Tab.Screen
        name="CommunityTab"
        component={CommunityScreen}
        options={{ title: strings.community, tabBarIcon: () => <TabIcon label="◎" /> }}
      />
      <Tab.Screen
        name="TryOnTab"
        component={TryOnScreen}
        options={{
          title: strings.tryOn,
          tabBarIcon: () => <TabIcon label="◉" large />,
        }}
      />
      <Tab.Screen
        name="WardrobeTab"
        component={WardrobeScreen}
        options={{ title: strings.wardrobe, tabBarIcon: () => <TabIcon label="▢" /> }}
      />
    </Tab.Navigator>
  );
}

function TabIcon({ label, large }: { label: string; large?: boolean }) {
  return (
    <Text style={{ color: colors.text, fontSize: large ? 26 : 18, fontWeight: large ? '700' : '500' }}>{label}</Text>
  );
}

export function RootNavigator() {
  return (
    <NavigationContainer theme={navTheme}>
      <Stack.Navigator
        screenOptions={{
          headerShown: false,
          contentStyle: { backgroundColor: colors.background },
        }}
      >
        <Stack.Screen name="Main" component={MainTabs} />
        <Stack.Screen
          name="Login"
          component={LoginRoute}
          options={{ presentation: 'modal', animation: 'slide_from_bottom' }}
        />
        <Stack.Screen
          name="Wishlist"
          component={WishlistScreen}
          options={{
            headerShown: true,
            title: strings.wishlist,
            headerStyle: { backgroundColor: colors.background },
            headerTintColor: colors.text,
            headerTitleStyle: { color: colors.text },
          }}
        />
        <Stack.Screen
          name="Profile"
          component={ProfileRoute}
          options={{
            headerShown: true,
            title: strings.profile,
            headerStyle: { backgroundColor: colors.background },
            headerTintColor: colors.text,
            headerTitleStyle: { color: colors.text },
          }}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
}

function LoginRoute({ navigation }: { navigation: { goBack: () => void } }) {
  return <LoginScreen onSuccess={() => navigation.goBack()} />;
}

function ProfileRoute({ navigation }: { navigation: { goBack: () => void } }) {
  return <ProfileScreen onLoggedOut={() => navigation.goBack()} />;
}
