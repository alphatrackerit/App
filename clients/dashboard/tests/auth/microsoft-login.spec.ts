import { expect, test, type Page } from "@playwright/test";

// MSAL's popup flow can't be driven in a route-mocked test — loginPopup opens a
// real login.microsoftonline.com window. So these specs cover the deterministic
// part: the "Continue with Microsoft" button is gated on a configured client id
// (env.msalClientId). The full popup → /token/microsoft exchange is verified
// manually against a real Entra app registration (see the feature docs).

async function setConfig(page: Page, extra: Record<string, unknown>) {
  await page.route("**/config.json", (route) =>
    route.fulfill({
      status: 200,
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ apiBase: "", defaultTenant: "root", demoMode: false, ...extra }),
    }),
  );
}

test.describe("login — Microsoft sign-in button", () => {
  test("is shown when a Microsoft client id is configured", async ({ page }) => {
    await setConfig(page, { msalClientId: "00000000-0000-0000-0000-000000000000" });
    await page.goto("/login");
    await expect(page.getByRole("button", { name: /continue with microsoft/i })).toBeVisible();
  });

  test("is hidden when no Microsoft client id is configured", async ({ page }) => {
    await setConfig(page, { msalClientId: "" });
    await page.goto("/login");
    await expect(page.getByRole("button", { name: /continue with microsoft/i })).toHaveCount(0);
  });
});
