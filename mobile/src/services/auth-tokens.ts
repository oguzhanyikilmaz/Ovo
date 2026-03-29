import * as SecureStore from 'expo-secure-store';

const ACCESS = 'ovo_access_token';
const REFRESH = 'ovo_refresh_token';

let accessMemory: string | null = null;

export async function loadTokensIntoMemory(): Promise<void> {
  accessMemory = await SecureStore.getItemAsync(ACCESS);
}

export function getAccessTokenSync(): string | null {
  return accessMemory;
}

export async function setTokens(access: string, refresh: string | null): Promise<void> {
  accessMemory = access;
  await SecureStore.setItemAsync(ACCESS, access);
  if (refresh) {
    await SecureStore.setItemAsync(REFRESH, refresh);
  }
}

export async function clearTokens(): Promise<void> {
  accessMemory = null;
  try {
    await SecureStore.deleteItemAsync(ACCESS);
  } catch {
    /* yoksa yoksay */
  }
  try {
    await SecureStore.deleteItemAsync(REFRESH);
  } catch {
    /* yoksa yoksay */
  }
}

export async function getRefreshToken(): Promise<string | null> {
  return SecureStore.getItemAsync(REFRESH);
}
