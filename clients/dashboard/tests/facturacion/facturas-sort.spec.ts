import { expect, test } from "@playwright/test";
import type { Route } from "@playwright/test";
import { mockJsonResponse } from "../helpers/api-mocks";
import { installShellMocks, paged } from "../helpers/shell-mocks";
import { seedAuthedSession, TEST_USER } from "../helpers/auth-seed";

// ───────────────────────────────────────────────────────────────────────
//  Ordenar por una columna debe ordenar TODO el dataset, no solo la página
//  visible. El servidor aquí pagina de verdad, y la factura con el mayor
//  pendiente está deliberadamente en la página 2: si el sort solo mirase las
//  filas cargadas, jamás aparecería al ordenar de forma descendente.
// ───────────────────────────────────────────────────────────────────────

/** 25 facturas. El pendiente crece con el índice, así que el máximo (25.000)
 *  es la última — fuera de la primera página de 20. */
const INVOICES = Array.from({ length: 25 }, (_, i) => ({
  id: `f-${i + 1}`,
  number: `FRA-${String(i + 1).padStart(3, "0")}`,
  dynamicsNumber: null,
  type: "Recibida",
  invoiceDate: "2026-01-15T00:00:00",
  dueDate: null,
  clientId: null,
  supplierId: "s-1",
  companyId: null,
  societyId: null,
  projectId: null,
  taxBase: null,
  vat: null,
  total: (i + 1) * 1000,
  paymentTerms: null,
  bank: null,
  statusId: null,
  verified: false,
  notes: null,
  documentPath: null,
  collected: 0,
  proformaId: null,
  verifactuStatus: "NoAplica",
  items: [],
}));

const PAGE_SIZES: number[] = [];

test.describe("facturas — sort sobre el dataset completo", () => {
  test.beforeEach(async ({ page }) => {
    PAGE_SIZES.length = 0;
    await seedAuthedSession(page, TEST_USER);
    await installShellMocks(page);
    await mockJsonResponse(page, "**/api/v1/cashflow/clients**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/suppliers**", paged([{ id: "s-1", name: "Proveedor Uno", code: null }]));
    await mockJsonResponse(page, "**/api/v1/cashflow/companies**", paged([]));
    await mockJsonResponse(page, "**/api/v1/cashflow/projects**", paged([]));

    // Un servidor que pagina de verdad: devuelve la porción que le piden.
    await page.route("**/api/v1/cashflow/invoices**", async (route: Route) => {
      const url = new URL(route.request().url());
      const pageNumber = Number(url.searchParams.get("pageNumber") ?? 1);
      const pageSize = Number(url.searchParams.get("pageSize") ?? 20);
      PAGE_SIZES.push(pageSize);
      const slice = INVOICES.slice((pageNumber - 1) * pageSize, pageNumber * pageSize);
      await route.fulfill({
        status: 200,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(
          paged(slice, { pageNumber, pageSize, totalCount: INVOICES.length }),
        ),
      });
    });
  });

  test("ordenar por Pendiente descendente saca el máximo global, no el de la página", async ({ page }) => {
    await page.goto("/facturacion/facturas");
    // Margen amplio: el chunk lazy de la página se compila en el primer acceso.
    await expect(page.getByRole("heading", { name: "Facturas" })).toBeVisible({ timeout: 20_000 });

    // Página 1 sin ordenar: sólo llegan FRA-001…FRA-020.
    await expect(page.getByText("FRA-001").last()).toBeVisible();
    await expect(page.getByText("FRA-025")).toHaveCount(0);

    // Ordenar por Pendiente: primer clic asc, segundo desc.
    // El nombre accesible del botón es su texto ("Pendiente"); apuntamos al title.
    const pendiente = page.locator('button[title="Ordenar por Pendiente"]');
    await pendiente.click({ timeout: 20_000 });
    await pendiente.click();

    // El mayor pendiente de TODAS las facturas (FRA-025 · 25.000) encabeza la tabla.
    await expect(page.getByText("FRA-025").last()).toBeVisible();
    await expect(page.getByText("25.000,00").last()).toBeVisible();

    // Y para lograrlo la página ha pedido el dataset completo al servidor.
    expect(Math.max(...PAGE_SIZES)).toBe(10000);
  });

  test("con el orden activo el paginador sigue troceando a 20 filas", async ({ page }) => {
    await page.goto("/facturacion/facturas");
    // Margen amplio: el chunk lazy de la página se compila en el primer acceso.
    await expect(page.getByRole("heading", { name: "Facturas" })).toBeVisible({ timeout: 20_000 });

    // El nombre accesible del botón es su texto ("Pendiente"); apuntamos al title.
    const pendiente = page.locator('button[title="Ordenar por Pendiente"]');
    await pendiente.click({ timeout: 20_000 });
    await pendiente.click();
    await expect(page.getByText("FRA-025").last()).toBeVisible();

    // 25 facturas / 20 por página = 2 páginas, y seguimos en la primera.
    await expect(page.getByText("Página 1 de 2")).toBeVisible();

    // La segunda página trae la cola del orden descendente: FRA-005…FRA-001.
    await page.getByRole("button", { name: "Next page" }).click();
    await expect(page.getByText("Página 2 de 2")).toBeVisible();
    await expect(page.getByText("FRA-001").last()).toBeVisible();
    await expect(page.getByText("FRA-025")).toHaveCount(0);
  });

  test("un filtro de columna también busca en todo el dataset", async ({ page }) => {
    await page.goto("/facturacion/facturas");
    // Margen amplio: el chunk lazy de la página se compila en el primer acceso.
    await expect(page.getByRole("heading", { name: "Facturas" })).toBeVisible({ timeout: 20_000 });

    // FRA-023 no está en la página 1; filtrarlo por número debe encontrarlo igual.
    await page.getByRole("button", { name: "Filtrar por Número" }).click();
    await page.getByPlaceholder("Filtrar número…").fill("FRA-023");

    await expect(page.getByText("FRA-023").last()).toBeVisible();
    await expect(page.getByText("FRA-001")).toHaveCount(0);
  });
});
