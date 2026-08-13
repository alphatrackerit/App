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

  test("facturas dialog: cuadre + quick-create + two-step generation confirm", async ({ page }) => {
    await page.goto("/facturacion/proformas");
    await page.getByRole("button", { name: /Facturas de la proforma PRO-2026-001/ }).click();

    const dialog = page.getByRole("dialog");
    await expect(dialog.getByText("Facturas de la proforma PRO-2026-001")).toBeVisible();
    await expect(dialog.getByText("PRO-2026-001-1")).toBeVisible();
    await expect(dialog.getByText("Pendiente", { exact: true })).toBeVisible();

    // Crear factura directa: el placeholder del importe propone el pendiente.
    await expect(dialog.getByText("Crear factura desde la proforma")).toBeVisible();
    await expect(dialog.getByLabel("Importe de la factura a crear")).toHaveAttribute("placeholder", "847,00");
    await expect(dialog.getByRole("button", { name: /Crear factura/ })).toBeEnabled();

    // Generación por plazos: con facturas ya vinculadas, el primer clic solo arma la confirmación.
    await dialog.getByRole("button", { name: "Generar facturas" }).click();
    await expect(dialog.getByText(/Pulsa otra vez para confirmar/)).toBeVisible();
    await expect(dialog.getByRole("button", { name: "Confirmar generación" })).toBeVisible();
  });

  test("quick-create POSTs the invoice with proforma data and links it", async ({ page }) => {
    let created: Record<string, unknown> | null = null;
    let linked: Record<string, unknown> | null = null;
    await page.route("**/api/v1/cashflow/invoices", async (route) => {
      if (route.request().method() !== "POST") return route.fallback();
      created = route.request().postDataJSON() as Record<string, unknown>;
      await route.fulfill({ status: 200, headers: { "Content-Type": "application/json" }, body: '"f-new"' });
    });
    await page.route("**/api/v1/cashflow/proformas/link-invoice", async (route) => {
      linked = route.request().postDataJSON() as Record<string, unknown>;
      await route.fulfill({ status: 204, body: "" });
    });

    await page.goto("/facturacion/proformas");
    await page.getByRole("button", { name: /Facturas de la proforma PRO-2026-001/ }).click();
    const dialog = page.getByRole("dialog");
    await expect(dialog.getByLabel("Importe de la factura a crear")).toBeVisible();
    await dialog.getByRole("button", { name: /Crear factura/ }).click();

    await expect(page.getByText("Factura creada en borrador y vinculada")).toBeVisible();
    expect(created).toMatchObject({
      type: "Emitida",
      number: "PRO-2026-001-2", // -1 ya existe → primer sufijo libre
      total: 847,               // pendiente por defecto
      clientId: "c-1",
      companyId: "e-1",
      paymentTerms: "30PP70-60D",
      taxBase: 700,             // 1000 × (847/1210)
      vat: 147,                 // 210 × (847/1210)
    });
    expect(linked).toMatchObject({ invoiceId: "f-new", proformaId: "pf-1" });
  });

  test("quick-create desde proforma Recibida envía number: null (borrador sin número)", async ({ page }) => {
    const RECIBIDA = {
      ...PROFORMA,
      id: "pf-2",
      number: "PF-REC-9",
      type: "Recibida",
      clientId: null,
      supplierId: "s-1",
    };
    await mockJsonResponse(page, "**/api/v1/cashflow/suppliers**", paged([{ id: "s-1", name: "Proveedor Uno", code: null }]));
    await mockJsonResponse(page, "**/api/v1/cashflow/proformas**", paged([RECIBIDA]));
    await mockJsonResponse(page, "**/api/v1/cashflow/proformas/*/lines", { ...LINES, proformaId: "pf-2", invoices: [] });

    let created: Record<string, unknown> | null = null;
    await page.route("**/api/v1/cashflow/invoices", async (route) => {
      if (route.request().method() !== "POST") return route.fallback();
      created = route.request().postDataJSON() as Record<string, unknown>;
      await route.fulfill({ status: 200, headers: { "Content-Type": "application/json" }, body: '"f-new"' });
    });
    await page.route("**/api/v1/cashflow/proformas/link-invoice", async (route) => {
      await route.fulfill({ status: 204, body: "" });
    });

    await page.goto("/facturacion/proformas");
    await page.getByRole("button", { name: /Facturas de la proforma PF-REC-9/ }).click();
    const dialog = page.getByRole("dialog");
    await expect(dialog.getByLabel("Importe de la factura a crear")).toBeVisible();
    await dialog.getByRole("button", { name: /Crear factura/ }).click();

    await expect(page.getByText("Factura creada en borrador y vinculada")).toBeVisible();
    expect(created).toMatchObject({ type: "Recibida", number: null, supplierId: "s-1" });
  });

  test("el filtro de pendientes manda el query param pending al servidor", async ({ page }) => {
    const pendingParams: (string | null)[] = [];
    await page.route("**/api/v1/cashflow/proformas?**", async (route) => {
      pendingParams.push(new URL(route.request().url()).searchParams.get("pending"));
      await route.fulfill({
        status: 200,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(paged([PROFORMA])),
      });
    });

    await page.goto("/facturacion/proformas");
    await expect(page.getByRole("heading", { name: "Proformas" })).toBeVisible({ timeout: 20_000 });
    expect(pendingParams.at(-1)).toBeNull();

    await page.getByRole("button", { name: "Sin factura" }).click();
    await expect.poll(() => pendingParams.at(-1)).toBe("SinFactura");

    await page.getByRole("button", { name: "Facturas sin número" }).click();
    await expect.poll(() => pendingParams.at(-1)).toBe("FacturasSinNumero");
    // Volver a "Todas" reutiliza la query cacheada sin pending — no dispara petición nueva.
  });

  test("el número de la factura vinculada es un acceso directo a Facturas", async ({ page }) => {
    // Listado vacío a propósito: la factura enlazada NO está en la página cargada,
    // así que solo puede abrirse porque Facturas la pide por id con ?factura=.
    await mockJsonResponse(page, "**/api/v1/cashflow/invoices**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/invoices/f-1", {
      id: "f-1",
      number: "PRO-2026-001-1",
      dynamicsNumber: null,
      type: "Emitida",
      invoiceDate: "2026-05-01T00:00:00",
      dueDate: null,
      clientId: "c-1",
      supplierId: null,
      companyId: "e-1",
      societyId: null,
      projectId: null,
      taxBase: 300,
      vat: 63,
      total: 363,
      paymentTerms: null,
      bank: null,
      statusId: null,
      verified: false,
      notes: null,
      documentPath: null,
      collected: 0,
      proformaId: "pf-1",
      verifactuStatus: "NoAplica",
      items: [],
    });

    await page.goto("/facturacion/proformas");
    await page.getByRole("button", { name: /Facturas de la proforma PRO-2026-001/ }).click();
    await page.getByRole("dialog").getByRole("button", { name: "Abrir la factura PRO-2026-001-1" }).click();

    const editor = page.getByRole("dialog");
    await expect(editor.getByText("Editar factura")).toBeVisible({ timeout: 20_000 });
    await expect(editor.locator("#fa-number")).toHaveValue("PRO-2026-001-1");
    // El parámetro se consume: recargar no reabre el diálogo.
    expect(new URL(page.url()).search).not.toContain("factura=");
  });
});
