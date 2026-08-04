import { expect, test } from "@playwright/test";
import { mockJsonResponse } from "../helpers/api-mocks";
import { installShellMocks } from "../helpers/shell-mocks";
import { seedAuthedSession, TEST_USER } from "../helpers/auth-seed";

const YEAR = new Date().getFullYear();

function day(date: string, totalIncomes: number, totalPayments: number) {
  return {
    date,
    totalIncomes,
    totalPayments,
    result: totalIncomes - totalPayments,
    accumulated: 0,
    colorHex: "#ffffff00",
    visualPriority: 2147483647,
    incomeDetails: [],
    paymentDetails: [],
  };
}

// Ledger spanning the year boundary: closing balance of Dec (prev year) = 1000,
// end of Jan = 1500, end of Feb = 1300.
const SUMMARY = [
  {
    projectId: "p1",
    projectName: "Proyecto Uno",
    clientName: "Cliente",
    companyName: "Empresa",
    countryName: "España",
    statusName: "EN CURSO",
    statusId: null,
    days: [
      day(`${YEAR - 1}-12-15T00:00:00`, 1000, 0),
      day(`${YEAR}-01-10T00:00:00`, 500, 0),
      day(`${YEAR}-02-05T00:00:00`, 0, 200),
    ],
  },
];

test.describe("flujo de caja (/flujo-de-caja)", () => {
  test.beforeEach(async ({ page }) => {
    await seedAuthedSession(page, TEST_USER);
    await installShellMocks(page);
    await mockJsonResponse(page, "**/api/v1/cashflow/reports/daily-summary**", SUMMARY);
  });

  test("renders the 'Caja mes ant.' row with each month's opening balance (Jan crosses the year)", async ({ page }) => {
    await page.goto("/flujo-de-caja");

    const row = page.getByRole("row").filter({ has: page.getByText("Caja mes ant.", { exact: true }) });
    await expect(row).toBeVisible();

    const cells = row.getByRole("cell");
    // Layout: [day label] + 3 cells (Pagos/Ingresos/Total) per month → Total of month m is cell 3*m.
    // Note: es-ES Intl only groups thousands from 10000 up, so 1000 renders as "1000,00".
    await expect(cells.nth(3)).toHaveText("1000,00"); // Enero ← 31/12 of previous year
    await expect(cells.nth(6)).toHaveText("1500,00"); // Febrero ← end of Enero
    await expect(cells.nth(9)).toHaveText("1300,00"); // Marzo ← end of Febrero
    // Pagos/Ingresos cells of the summary row stay empty.
    await expect(cells.nth(1)).toHaveText("");
    await expect(cells.nth(2)).toHaveText("");
  });

  test("opening-balance row matches the year-end balance shown in the hero", async ({ page }) => {
    await page.goto("/flujo-de-caja");
    // Hero: Saldo fin de año = 1300 (full-ledger accumulated through Dec 31).
    await expect(page.getByText("Saldo fin de año:")).toBeVisible();
    await expect(page.getByText("1300,00").first()).toBeVisible();
  });
});
