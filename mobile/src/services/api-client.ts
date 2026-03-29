import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { API_BASE_URL, ABP_TENANT_ID, OIDC_CLIENT_ID, OIDC_CLIENT_SECRET, OIDC_SCOPE, TOKEN_ENDPOINT } from '@/constants/config';
import {
  clearTokens,
  getAccessTokenSync,
  getRefreshToken,
  setTokens,
} from '@/services/auth-tokens';

export type ApiClient = ReturnType<typeof createApiClient>;

function parseAbpMessage(data: unknown): string | null {
  if (data && typeof data === 'object' && 'error' in data) {
    const raw = data as {
      error?: string | { message?: string; details?: string };
      error_description?: string;
    };
    if (typeof raw.error === 'string') {
      const desc = raw.error_description?.trim();
      return desc ? `${raw.error}: ${desc}` : raw.error;
    }
    const err = raw.error;
    return err && typeof err === 'object' ? err.message ?? err.details ?? null : null;
  }
  return null;
}

export function getErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const ax = error as AxiosError<{
      error?: string | { message?: string };
      error_description?: string;
    }>;
    const msg =
      parseAbpMessage(ax.response?.data) ??
      (typeof ax.response?.data?.error === 'object'
        ? ax.response.data.error?.message
        : undefined);
    if (msg) {
      return msg;
    }
    if (ax.message) {
      return ax.message;
    }
  }
  if (error instanceof Error) {
    return error.message;
  }
  return fallback;
}

let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  if (refreshPromise) {
    return refreshPromise;
  }
  refreshPromise = (async () => {
    const rt = await getRefreshToken();
    if (!rt) {
      return null;
    }
    const body = new URLSearchParams({
      grant_type: 'refresh_token',
      refresh_token: rt,
      client_id: OIDC_CLIENT_ID,
      scope: OIDC_SCOPE,
    });
    if (OIDC_CLIENT_SECRET) {
      body.set('client_secret', OIDC_CLIENT_SECRET);
    }
    try {
      const { data } = await axios.post<{
        access_token: string;
        refresh_token?: string;
      }>(TOKEN_ENDPOINT, body, {
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      });
      await setTokens(data.access_token, data.refresh_token ?? rt);
      return data.access_token;
    } catch {
      await clearTokens();
      return null;
    } finally {
      refreshPromise = null;
    }
  })();
  return refreshPromise;
}

export function createApiClient() {
  const client = axios.create({
    baseURL: API_BASE_URL,
    timeout: 30_000,
    headers: {
      'Content-Type': 'application/json',
    },
  });

  client.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    const t = getAccessTokenSync();
    if (t) {
      config.headers.Authorization = `Bearer ${t}`;
    }
    if (ABP_TENANT_ID) {
      config.headers['Abp-TenantId'] = ABP_TENANT_ID;
    }
    return config;
  });

  client.interceptors.response.use(
    (r) => r,
    async (error: AxiosError) => {
      const status = error.response?.status;
      const original = error.config;
      if (
        status === 401 &&
        original &&
        !(original as { _retry?: boolean })._retry
      ) {
        (original as { _retry?: boolean })._retry = true;
        const newToken = await refreshAccessToken();
        if (newToken) {
          original.headers = original.headers ?? {};
          original.headers.Authorization = `Bearer ${newToken}`;
          return client.request(original);
        }
      }
      return Promise.reject(error);
    }
  );

  return client;
}

export const apiClient = createApiClient();
