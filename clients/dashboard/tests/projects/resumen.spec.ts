import { expect, test } from "@playwright/test";
import { mockJsonResponse } from "../helpers/api-mocks";
import { installShellMocks, paged } from "../helpers/shell-mocks";
import { seedAuthedSession, TEST_USER } from "../helpers/auth-seed";

function isoAt(daysFromToday: number): string {
  const now = new Date();
  const dt = new Date(now.getFullYear(), now.getMonth(), now.getDate() + daysFromToday);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${dt.getFullYear()}-${pad(dt.getMonth() + 1)}-${pad(dt.getDate())}T00:00:00`;
}

function detail(id: string, amount: number, confirmed: boolean) {
  return { id, amount, percentage: null, status: "PENDIENTE", description: null, confirmed, validated: false };
}

function day(date: string, incomes: ReturnType<typeof detail>[], payments: ReturnType<typeof detail>[]) {
  const inc = incomes.reduce((s, d) => s + d.amount, 0);
  const pay = payments.reduce((s, d) => s + d.amount, 0);
  return {
    date,
    totalIncomes: inc,
    totalPayments: pay,
    result: inc - pay,
    accumulated: 0,
    colorHex: "#ffffff00",
    visualPriority: 2147483647,
    incomeDetails: incomes,
    paymentDetails: payments.map((p) => ({ ...p, supplierName: "Prov" })),
  };
}

// Yesterday: +1000 confirmed (p1). In 10 days: -300 unconfirmed (p2). In 20 days: +200 unconfirmed (p1).
// → saldo hoy 1000 · vencimientos 90d 300 · sin confirmar 30d neto -100 · runway ∞ (never negative).
const SUMMARY = [
  {
    projectId: "p1",
    projectName: "Proyecto Uno",
    clientName: "Cliente",
    companyName: "Empresa Uno",
    countryName: "España",
    statusName: "EN CURSO",
    statusId: null,
    days: [day(isoAt(-1), [detail("i1", 1000, true)], []), day(isoAt(20), [detail("i2", 200, false)], [])],
  },
  {
    projectId: "p2",
    projectName: "Proyecto Dos",
    clientName: "Cliente",
    companyName: "Empresa Dos",
    countryName: "España",
    statusName: "EN CURSO",
    statusId: null,
    days: [day(isoAt(10), [], [detail("g1", 300, false)])],
  },
];

const PROJECTS = paged([
  { id: "p1", name: "Proyecto Uno", salePrice: 5000, forecastSale: null, cost: null, forecastCost: null, profit: null, clientId: null, societyId: null, countryId: null, companyId: "c1", statusId: null, prefixId: null },
  { id: "p2", name: "Proyecto Dos", salePrice: 3000, forecastSale: null, cost: null, forecastCost: null, profit: null, clientId: null, societyId: null, countryId: null, companyId: "c2", statusId: null, prefixId: null },
]);

const COMPANIES = paged([
  { id: "c1", name: "Empresa Uno", code: null },
  { id: "c2", name: "Empresa Dos", code: null },
]);

test.describe("resumen (/resumen)", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuthedSession(page, TEST_USER);
    await installShellMocks(page);
    await mockJsonResponse(page, "**/api/v1/cashflow/reports/daily-summary**", SUMMARY);
    await mockJsonResponse(page, "**/api/v1/cashflow/projects**", PROJECTS);
    await mockJsonResponse(page, "**/api/v1/cashflow/companies**", COMPANIES);
  });

  test("renders the KPI cards with computed values", async ({ page }) => {
    await page.goto("/resumen");
    await expect(page.getByRole("heading", { name: "Resumen" })).toBeVisible();

    await expect(page.getByText("Saldo de caja (hoy)")).toBeVisible();
    await expect(page.getByText("1000 €").first()).toBeVisible();

    await expect(page.getByText("Vencimientos próx. 90 días")).toBeVisible();
    await expect(page.getByText("300 €").first()).toBeVisible();

    // Never goes negative → infinite runway.
    await expect(page.getByText("Runway")).toBeVisible();
    await expect(page.getByText("∞")).toBeVisible();

    // Unconfirmed within 30 days: +200 income, -300 payment. Depending on the run date the
    // "Neto del mes" card can legitimately show the same -100 € — assert presence, not uniqueness.
    await expect(page.getByText("Previsto sin confirmar (30d)")).toBeVisible();
    await expect(page.getByText("-100 €").first()).toBeVisible();
  });

  test("shows per-company balances and the ventas toggle", async ({ page }) => {
    await page.goto("/resumen");

    // Only movements up to today count: Empresa Uno holds the confirmed +1000.
    await expect(page.getByText("Por empresa · saldo actual")).toBeVisible();
    await expect(page.getByText("Empresa Uno")).toBeVisible();

    // Ventas: contratado = 5000 + 3000.
    await expect(page.getByText("Total ventas")).toBeVisible();
    await expect(page.getByText("8000 €")).toBeVisible();
    await expect(page.getByText(/Cobrado real/)).toBeVisible();

    // Toggle "Contratado" hides the real figure.
    await page.getByRole("button", { name: "Contratado", exact: true }).click();
    await expect(page.getByText(/Cobrado real/)).toBeHidden();
  });
});
