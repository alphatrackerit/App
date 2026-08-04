import { expect, test } from "@playwright/test";
import { mockJsonResponse } from "../helpers/api-mocks";
import { installShellMocks, paged } from "../helpers/shell-mocks";
import { seedAuthedSession, TEST_USER } from "../helpers/auth-seed";

const PROFORMA = {
  id: "pf-1",
  number: "PRO-2026-001",
  type: "Emitida",
  date: "2026-05-01T00:00:00",
  clientId: "c-1",
  supplierId: null,
  companyId: "e-1",
  societyId: null,
  projectId: null,
  taxBase: 1000,
  vat: 210,
  total: 1210,
  paymentTerms: "30PP70-60D",
  statusId: null,
  notes: null,
  documentPath: null,
  invoiced: 363,
};

const LINES = {
  proformaId: "pf-1",
  total: 1210,
  invoicedAmount: 363,
  pending: 847,
  invoices: [
    { id: "f-1", number: "PRO-2026-001-1", invoiceDate: "2026-05-01T00:00:00", dueDate: null, total: 363, statusId: null },
  ],
};

test.describe("proformas (/facturacion/proformas)", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuthedSession(page, TEST_USER);
    await installShellMocks(page);
    await mockJsonResponse(page, "**/api/v1/cashflow/clients**", paged([{ id: "c-1", name: "Cliente Uno", code: null }]));
    await mockJsonResponse(page, "**/api/v1/cashflow/suppliers**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/companies**", paged([{ id: "e-1", name: "Empresa Uno", code: null }]));
    await mockJsonResponse(page, "**/api/v1/cashflow/societies**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/statuses**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/projects**", paged([]));
    // Registered LAST so the more specific /lines route wins over the broad search glob.
    await mockJsonResponse(page, "**/api/v1/cashflow/proformas**", paged([PROFORMA]));
    await mockJsonResponse(page, "**/api/v1/cashflow/proformas/*/lines", LINES);
  });

  test("lists proformas with amounts and counterparty", async ({ page }) => {
    await page.goto("/facturacion/proformas");

    await expect(page.getByRole("heading", { name: "Proformas" })).toBeVisible();
    // .last() — the hidden mobile card duplicates each text in the DOM; the desktop row comes last.
    await expect(page.getByText("PRO-2026-001").last()).toBeVisible();
    await expect(page.getByText("Cliente Uno").last()).toBeVisible();
    // Total 1210 · Facturado 363 · Pendiente 847
    await expect(page.getByText("1210,00").last()).toBeVisible();
    await expect(page.getByText("847,00").last()).toBeVisible();
  });

  test("facturas dialog shows linked invoices as an informative cuadre (no generation)", async ({ page }) => {
    await page.goto("/facturacion/proformas");
    await page.getByRole("button", { name: /Facturas de la proforma PRO-2026-001/ }).click();

    const dialog = page.getByRole("dialog");
    await expect(dialog.getByText("Facturas de la proforma PRO-2026-001")).toBeVisible();
    await expect(dialog.getByText("PRO-2026-001-1")).toBeVisible();
    await expect(dialog.getByText("Pendiente", { exact: true })).toBeVisible();

    // La generación automática se retiró de la UI: la asociación se hace desde la factura.
    await expect(dialog.getByRole("button", { name: "Generar facturas" })).toHaveCount(0);
    await expect(dialog.getByRole("button", { name: "Cerrar" })).toBeVisible();
  });
});
