import { apiFetch } from "@/lib/api-client";

export type TokenResponse = {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
};

export function issueToken(input: {
  email: string;
  password: string;
  tenant: string;
}) {
  return apiFetch<TokenResponse>("/api/v1/identity/token/issue", {
    method: "POST",
    body: JSON.stringify({ email: input.email, password: input.password }),
    // X-FSH-App tells the API this credential request originated from the
    // tenant dashboard. The server uses it to enforce the SuperAdmin / app
    // boundary — a root-tenant login submitted with X-FSH-App=dashboard is
    // rejected with 403 instead of receiving a usable token.
    headers: { tenant: input.tenant, "X-FSH-App": "dashboard" },
    skipAuth: true,
  });
}

/**
 * Exchanges a Microsoft (Entra ID) ID token — obtained via MSAL — for an FSH
 * token pair. No tenant header: the server derives the tenant from the verified
 * email domain. Only existing, active users are accepted (link-only).
 */
export function issueMicrosoftToken(idToken: string) {
  return apiFetch<TokenResponse>("/api/v1/identity/token/microsoft", {
    method: "POST",
    body: JSON.stringify({ idToken }),
    headers: { "X-FSH-App": "dashboard" },
    skipAuth: true,
  });
}
