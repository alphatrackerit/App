import { expect, test } from "@playwright/test";
import { mockJsonResponse } from "../helpers/api-mocks";
import { installShellMocks, paged } from "../helpers/shell-mocks";
import { seedAuthedSession, TEST_USER } from "../helpers/auth-seed";

// ───────────────────────────────────────────────────────────────────────
//  Vencimientos: pagos por porcentajes (líneas reales) + hitos PREVISTOS
//  derivados de la forma de pago cuando la factura no tiene líneas, y
//  filtros por proveedor / estado / fechas.
// ───────────────────────────────────────────────────────────────────────

const baseInvoice = {
  dynamicsNumber: null,
  clientId: null,
  societyId: null,
  projectId: null,
  taxBase: null,
  vat: null,
  bank: null,
  statusId: null,
  verified: false,
  notes: null,
  documentPath: null,
  collected: 0,
  proformaId: null,
  verifactuStatus: "NoAplica",
  items: [],
};

// Factura con líneas de caja reales 30/70 (la primera confirmada = pagada).
const FRA_CON_LINEAS = {
  ...baseInvoice,
  id: "f-1",
  number: "FR-100",
  type: "Recibida",
  invoiceDate: "2026-06-01T00:00:00",
  dueDate: "2026-08-01T00:00:00",
  supplierId: "s-1",
  companyId: "e-1",
  total: 1000,
  paymentTerms: "30PP70-60D",
};

// Factura Recibida en borrador (sin número) SIN líneas → hitos previstos 30/70.
const FRA_SIN_LINEAS = {
  ...baseInvoice,
  id: "f-2",
  number: null,
  type: "Recibida",
  invoiceDate: "2026-07-01T00:00:00",
  dueDate: null,
  supplierId: "s-2",
  companyId: "e-1",
  total: 500,
  paymentTerms: "30PP70-60D",
};

const PAYMENTS = [
  {
    id: "p-1", amount: 300, description: "FR-100", date: "2026-06-01T00:00:00", percentage: 30,
    supplierId: "s-1", projectId: null, statusId: null, invoiceId: "f-1", confirmed: true, validated: true,
  },
  {
    id: "p-2", amount: 700, description: "FR-100", date: "2026-08-01T00:00:00", percentage: 70,
    supplierId: "s-1", projectId: null, statusId: null, invoiceId: "f-1", confirmed: false, validated: false,
  },
];

test.describe("vencimientos (/facturacion/vencimientos)", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuthedSession(page, TEST_USER);
    await installShellMocks(page);
    await mockJsonResponse(page, "**/api/v1/cashflow/clients**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/suppliers**", paged([
      { id: "s-1", name: "Proveedor Uno", code: null },
      { id: "s-2", name: "Proveedor Dos", code: null },
    ]));
    await mockJsonResponse(page, "**/api/v1/cashflow/companies**", paged([{ id: "e-1", name: "Empresa Uno", code: null }]));
    await mockJsonResponse(page, "**/api/v1/cashflow/projects**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/incomes**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/payments**", paged(PAYMENTS));
    await mockJsonResponse(page, "**/api/v1/cashflow/invoices**", paged([FRA_CON_LINEAS, FRA_SIN_LINEAS]));
  });

  test("muestra pagos con porcentaje y estado por hito", async ({ page }) => {
    await page.goto("/facturacion/vencimientos");
    await expect(page.getByRole("heading", { name: "Vencimientos" })).toBeVisible({ timeout: 20_000 });

    // Líneas reales: 30 % pagado, 70 % pendiente.
    await expect(page.getByText("FR-100/1")).toBeVisible();
    await expect(page.getByText("FR-100/2")).toBeVisible();
    await expect(page.getByText("30 %").first()).toBeVisible();
    await expect(page.getByText("Pagada", { exact: true })).toBeVisible();
    await expect(page.getByText("Pago 1/2").first()).toBeVisible();
  });

  test("una factura sin líneas proyecta hitos Previsto desde la forma de pago", async ({ page }) => {
    await page.goto("/facturacion/vencimientos");
    await expect(page.getByRole("heading", { name: "Vencimientos" })).toBeVisible({ timeout: 20_000 });

    // 30PP70-60D sobre 500 → 150 (anticipo) + 350 (a 60 días: 2026-08-30).
    await expect(page.getByText("(sin número)/1")).toBeVisible();
    await expect(page.getByText("(sin número)/2")).toBeVisible();
    await expect(page.getByText("150,00 €").first()).toBeVisible();
    await expect(page.getByText("350,00 €").first()).toBeVisible();
    await expect(page.getByText("30/8/2026")).toBeVisible();
    expect(await page.getByText("Previsto", { exact: true }).count()).toBeGreaterThanOrEqual(2);
    // Un hito previsto no tiene línea de caja: no ofrece "Registrar".
    const filaPrevisto = page.locator("li", { hasText: "(sin número)/1" });
    await expect(filaPrevisto.getByRole("button", { name: "Registrar" })).toHaveCount(0);
  });

  test("filtra por proveedor, estado y rango de fechas", async ({ page }) => {
    await page.goto("/facturacion/vencimientos");
    await expect(page.getByRole("heading", { name: "Vencimientos" })).toBeVisible({ timeout: 20_000 });

    // Proveedor Dos → solo los hitos previstos de la factura sin número.
    await page.getByRole("button", { name: /^Proveedor$/ }).click();
    await page.getByRole("menuitemradio", { name: "Proveedor Dos" }).click();
    await expect(page.getByText("(sin número)/1")).toBeVisible();
    await expect(page.getByText("FR-100/1")).toHaveCount(0);

    // Quitar proveedor y filtrar por estado Pagado → solo FR-100/1.
    await page.getByRole("button", { name: "Quitar filtro de proveedor" }).click();
    await page.getByLabel("Estado", { exact: true }).getByRole("button", { name: "Pagado" }).click();
    await expect(page.getByText("FR-100/1")).toBeVisible();
    await expect(page.getByText("FR-100/2")).toHaveCount(0);

    // Rango de fechas: solo agosto 2026 → FR-100/2 y el hito a 60 días.
    await page.getByRole("button", { name: "Todos", exact: true }).click();
    await page.getByLabel("Vence desde").fill("2026-08-01");
    await page.getByLabel("Vence hasta").fill("2026-08-31");
    await expect(page.getByText("FR-100/2")).toBeVisible();
    await expect(page.getByText("FR-100/1")).toHaveCount(0);
  });
});
