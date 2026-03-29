/**
 * Swagger/OpenAPI: ../../docs/openapi/swagger.web.v1.full.json
 * API öneki: /api/app/* ve OpenIddict: /connect/token
 */
const trimSlash = (u: string) => u.replace(/\/+$/, '');

export const API_BASE_URL = trimSlash(
  process.env.EXPO_PUBLIC_API_URL ?? 'https://localhost:44349'
);

export const OIDC_CLIENT_ID = process.env.EXPO_PUBLIC_OIDC_CLIENT_ID ?? 'OVO_App';
export const OIDC_CLIENT_SECRET = process.env.EXPO_PUBLIC_OIDC_CLIENT_SECRET ?? '';
export const OIDC_SCOPE = process.env.EXPO_PUBLIC_OIDC_SCOPE ?? 'OVO offline_access';

export const ABP_TENANT_ID = process.env.EXPO_PUBLIC_ABP_TENANT_ID?.trim() || undefined;

export const TOKEN_ENDPOINT = `${API_BASE_URL}/connect/token`;
