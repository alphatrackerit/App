import { PublicClientApplication, type IPublicClientApplication } from "@azure/msal-browser";
import { env } from "@/env";

// Thin wrapper around MSAL so the rest of the app never imports @azure/msal-browser
// directly — keeps the popup flow mockable in tests and the dependency in one place.
// The dashboard owns its own auth-context, so we drive MSAL imperatively (no MsalProvider).

let initPromise: Promise<IPublicClientApplication> | null = null;

/** True when Microsoft sign-in is configured (a client id is present in runtime config). */
export function isMicrosoftSignInEnabled(): boolean {
  return env.msalClientId.length > 0;
}

async function getInstance(): Promise<IPublicClientApplication> {
  initPromise ??= (async () => {
    const pca = new PublicClientApplication({
      auth: {
        clientId: env.msalClientId,
        authority: env.msalAuthority,
        redirectUri: window.location.origin,
      },
      // sessionStorage: the ID token is exchanged for an FSH token immediately,
      // so MSAL's own cache doesn't need to outlive the tab.
      cache: { cacheLocation: "sessionStorage" },
    });
    await pca.initialize();
    return pca;
  })();
  return initPromise;
}

/**
 * Opens the Microsoft sign-in popup and returns the raw ID token, which the backend
 * exchanges for an FSH access/refresh pair. Rejects if the user closes the popup.
 */
export async function signInWithMicrosoftPopup(): Promise<{ idToken: string }> {
  const pca = await getInstance();
  const result = await pca.loginPopup({
    scopes: ["openid", "profile", "email"],
    prompt: "select_account",
  });
  if (!result.idToken) {
    throw new Error("Microsoft did not return an ID token.");
  }
  return { idToken: result.idToken };
}
