import axios from 'axios';
import { OIDC_CLIENT_ID, OIDC_CLIENT_SECRET, OIDC_SCOPE, TOKEN_ENDPOINT } from '@/constants/config';
import type { TokenResponse } from '@/types/ovo-api';
import { clearTokens, setTokens } from '@/services/auth-tokens';

export async function loginWithPassword(userNameOrEmail: string, password: string): Promise<TokenResponse> {
  const body = new URLSearchParams({
    grant_type: 'password',
    username: userNameOrEmail,
    password,
    client_id: OIDC_CLIENT_ID,
    scope: OIDC_SCOPE,
  });
  if (OIDC_CLIENT_SECRET) {
    body.set('client_secret', OIDC_CLIENT_SECRET);
  }
  const { data } = await axios.post<TokenResponse>(TOKEN_ENDPOINT, body, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  });
  await setTokens(data.access_token, data.refresh_token ?? null);
  return data;
}

export async function logout(): Promise<void> {
  await clearTokens();
}
