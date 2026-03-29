import { create } from 'zustand';

interface AuthState {
  hydrated: boolean;
  isLoggedIn: boolean;
  userLabel: string | null;
  setHydrated: (v: boolean) => void;
  setSession: (loggedIn: boolean, userLabel: string | null) => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  hydrated: false,
  isLoggedIn: false,
  userLabel: null,
  setHydrated: (v) => set({ hydrated: v }),
  setSession: (loggedIn, userLabel) => set({ isLoggedIn: loggedIn, userLabel }),
}));
