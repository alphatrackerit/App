import { expect, test } from "@playwright/test";
import { mockJsonResponse } from "../helpers/api-mocks";
import { installShellMocks, paged } from "../helpers/shell-mocks";
import { seedAuthedSession, TEST_USER } from "../helpers/auth-seed";

const COMPANY = {
  id: "e-1",
  name: "Empresa Uno",
  code: null,
  legalName: "Empresa Uno SL",
  taxRegistration: null,
  showInProjects: true,
  nif: "B12345678",
};

const SETTINGS_SIN_CERT = {
  companyId: "e-1",
  environment: "Pruebas",
  enabled: false,
  softwareName: null,
  softwareVersion: null,
  installationNumber: null,
  hasCertificate: false,
  lastChainHash: null,
  lastChainAtUtc: null,
};

const SETTINGS_CON_CADENA = {
  ...SETTINGS_SIN_CERT,
  enabled: true,
  hasCertificate: true,
  lastChainHash: "3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60",
  lastChainAtUtc: "2026-07-31T10:00:00+00:00",
};

test.describe("verifactu (/facturacion/verifactu)", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuthedSession(page, TEST_USER);
    await installShellMocks(page);
    await mockJsonResponse(page, "**/api/v1/cashflow/companies**", paged([COMPANY]));
  });

  test("without certificate: warns, disables the send switch, shows empty chain", async ({ page }) => {
    await mockJsonResponse(page, "**/api/v1/cashflow/verifactu/settings/**", SETTINGS_SIN_CERT);
    await page.goto("/facturacion/verifactu");

    await expect(page.getByRole("heading", { name: "VeriFactu" })).toBeVisible();
    await expect(page.getByText("Sin certificado")).toBeVisible();
    await expect(page.getByText("Envío desactivado")).toBeVisible();
    // NIF de la empresa precargado desde el catálogo.
    await expect(page.locator("#vf-nif")).toHaveValue("B12345678");
    // Sin certificado no se puede activar el envío.
    await expect(page.getByRole("switch", { name: "Envío a la AEAT activo" })).toBeDisabled();
    await expect(page.getByText(/la cadena empieza con la primera factura/i)).toBeVisible();
  });

  test("with certificate and chain: shows badges and the last hash", async ({ page }) => {
    await mockJsonResponse(page, "**/api/v1/cashflow/verifactu/settings/**", SETTINGS_CON_CADENA);
    await page.goto("/facturacion/verifactu");

    await expect(page.getByText("Certificado guardado", { exact: true })).toBeVisible();
    await expect(page.getByText("Envío activo")).toBeVisible();
    await expect(page.getByText("3C464DAF61ACB827C65FDA19F352A4E3BDC2C640E9E9FC4CC058073F38F12F60")).toBeVisible();
    await expect(page.getByRole("switch", { name: "Envío a la AEAT activo" })).toBeEnabled();
  });
});
