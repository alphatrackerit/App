import { apiFetch } from "@/lib/api-client";

// ───────────────────────────────────────────────────────────────────────
//  Shared
// ───────────────────────────────────────────────────────────────────────

export type PagedResponse<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
};

export type SortDir = "asc" | "desc";

type PagedParams = {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: SortDir;
};

function pagedQuery(params: PagedParams & Record<string, unknown>): string {
  const q = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === "") continue;
    q.set(key, String(value));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

// ───────────────────────────────────────────────────────────────────────
//  Projects
// ───────────────────────────────────────────────────────────────────────

export type ProjectDto = {
  id: string;
  name: string;
  salePrice: number | null;
  forecastSale: number | null;
  cost: number | null;
  forecastCost: number | null;
  profit: number | null;
  clientId: string | null;
  societyId: string | null;
  countryId: string | null;
  companyId: string | null;
  statusId: string | null;
  prefixId: string | null;
};

export type ProjectInput = {
  name: string;
  salePrice?: number | null;
  forecastSale?: number | null;
  cost?: number | null;
  forecastCost?: number | null;
  profit?: number | null;
  clientId?: string | null;
  societyId?: string | null;
  countryId?: string | null;
  companyId?: string | null;
  statusId?: string | null;
  prefixId?: string | null;
};

export function searchProjects(
  params: PagedParams & { companyId?: string } = {},
): Promise<PagedResponse<ProjectDto>> {
  return apiFetch<PagedResponse<ProjectDto>>(`/api/v1/cashflow/projects${pagedQuery(params)}`);
}

export function getProject(id: string): Promise<ProjectDto> {
  return apiFetch<ProjectDto>(`/api/v1/cashflow/projects/${encodeURIComponent(id)}`);
}

export function createProject(input: ProjectInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/projects", { method: "POST", body: JSON.stringify(input) });
}

export function updateProject(id: string, input: ProjectInput): Promise<string> {
  return apiFetch<string>(`/api/v1/cashflow/projects/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteProject(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/projects/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ───────────────────────────────────────────────────────────────────────
//  Incomes
// ───────────────────────────────────────────────────────────────────────

export type IncomeDto = {
  id: string;
  amount: number;
  description: string | null;
  date: string | null;
  percentage: number | null;
  projectId: string | null;
  statusId: string | null;
  invoiceId: string | null;
  confirmed: boolean;
  validated: boolean;
};

export type IncomeInput = {
  amount: number;
  description?: string | null;
  date?: string | null;
  percentage?: number | null;
  projectId?: string | null;
  statusId?: string | null;
  invoiceId?: string | null;
  confirmed?: boolean;
};

export function searchIncomes(
  params: PagedParams & { projectId?: string; unlinked?: boolean } = {},
): Promise<PagedResponse<IncomeDto>> {
  return apiFetch<PagedResponse<IncomeDto>>(`/api/v1/cashflow/incomes${pagedQuery(params)}`);
}

export function createIncome(input: IncomeInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/incomes", { method: "POST", body: JSON.stringify(input) });
}

export function updateIncome(id: string, input: IncomeInput): Promise<string> {
  return apiFetch<string>(`/api/v1/cashflow/incomes/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteIncome(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/incomes/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export async function confirmIncome(id: string, value = true): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/incomes/${encodeURIComponent(id)}/confirm?value=${value}`, { method: "POST" });
}

export async function validateIncome(id: string, value = true): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/incomes/${encodeURIComponent(id)}/validate?value=${value}`, { method: "POST" });
}

// ───────────────────────────────────────────────────────────────────────
//  Payments
// ───────────────────────────────────────────────────────────────────────

export type PaymentDto = {
  id: string;
  amount: number;
  description: string | null;
  date: string | null;
  percentage: number | null;
  supplierId: string | null;
  projectId: string | null;
  statusId: string | null;
  invoiceId: string | null;
  confirmed: boolean;
  validated: boolean;
};

export type PaymentInput = {
  amount: number;
  description?: string | null;
  date?: string | null;
  percentage?: number | null;
  supplierId?: string | null;
  projectId?: string | null;
  statusId?: string | null;
  invoiceId?: string | null;
  confirmed?: boolean;
};

export function searchPayments(
  params: PagedParams & { projectId?: string; unlinked?: boolean } = {},
): Promise<PagedResponse<PaymentDto>> {
  return apiFetch<PagedResponse<PaymentDto>>(`/api/v1/cashflow/payments${pagedQuery(params)}`);
}

export function createPayment(input: PaymentInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/payments", { method: "POST", body: JSON.stringify(input) });
}

export function updatePayment(id: string, input: PaymentInput): Promise<string> {
  return apiFetch<string>(`/api/v1/cashflow/payments/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deletePayment(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/payments/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export async function confirmPayment(id: string, value = true): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/payments/${encodeURIComponent(id)}/confirm?value=${value}`, { method: "POST" });
}

export async function validatePayment(id: string, value = true): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/payments/${encodeURIComponent(id)}/validate?value=${value}`, { method: "POST" });
}

// ───────────────────────────────────────────────────────────────────────
//  Notes
// ───────────────────────────────────────────────────────────────────────

export type NoteDto = {
  id: string;
  projectId: string | null;
  title: string;
  description: string | null;
  date: string;
};

export type NoteInput = {
  title: string;
  description?: string | null;
  date?: string | null;
  projectId?: string | null;
};

export function searchNotes(
  params: PagedParams & { projectId?: string } = {},
): Promise<PagedResponse<NoteDto>> {
  return apiFetch<PagedResponse<NoteDto>>(`/api/v1/cashflow/notes${pagedQuery(params)}`);
}

export function createNote(input: NoteInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/notes", { method: "POST", body: JSON.stringify(input) });
}

export function updateNote(id: string, input: NoteInput): Promise<string> {
  return apiFetch<string>(`/api/v1/cashflow/notes/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteNote(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/notes/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ───────────────────────────────────────────────────────────────────────
//  Cash flow (aggregate ledger for the Flujo de caja grid)
// ───────────────────────────────────────────────────────────────────────

export type CashflowEntry = {
  id: string;
  projectId: string | null;
  date: string; // ISO date-time
  amount: number;
  description: string | null;
  confirmed: boolean;
  validated: boolean;
};

export type CashflowProject = { id: string; name: string };

export type Cashflow = {
  projects: CashflowProject[];
  incomes: CashflowEntry[];
  payments: CashflowEntry[];
};

export function getCashflow(): Promise<Cashflow> {
  return apiFetch<Cashflow>("/api/v1/cashflow/cashflow");
}

// ───────────────────────────────────────────────────────────────────────
//  Daily summary (server-side report — spec §6)
// ───────────────────────────────────────────────────────────────────────

export type DailyIncomeDetail = {
  id: string;
  amount: number;
  percentage: number;
  status: string;
  description: string | null;
  confirmed: boolean;
  validated: boolean;
};

export type DailyPaymentDetail = DailyIncomeDetail & { supplierName: string };

export type DailySummaryDay = {
  date: string; // ISO date-time, no offset
  totalIncomes: number;
  totalPayments: number;
  result: number; // totalIncomes - totalPayments
  accumulated: number; // running balance for this project
  colorHex: string; // predominant-supplier color; "#ffffff00" if none
  visualPriority: number; // that supplier's priority; int.MaxValue if none
  incomeDetails: DailyIncomeDetail[];
  paymentDetails: DailyPaymentDetail[];
};

export type DailySummaryProject = {
  projectId: string;
  projectName: string;
  clientName: string;
  companyName: string;
  countryName: string;
  statusName: string;
  statusId: string | null;
  days: DailySummaryDay[];
};

export type DailySummaryParams = {
  from?: string;
  to?: string;
  companyIds?: string[];
  projectIds?: string[];
  statusIds?: string[];
  onlyConfirmed?: boolean;
  onlyValidated?: boolean;
};

export function getDailySummary(params: DailySummaryParams = {}): Promise<DailySummaryProject[]> {
  const q = new URLSearchParams();
  if (params.from) q.set("from", params.from);
  if (params.to) q.set("to", params.to);
  for (const id of params.companyIds ?? []) q.append("companyIds", id);
  for (const id of params.projectIds ?? []) q.append("projectIds", id);
  for (const id of params.statusIds ?? []) q.append("statusIds", id);
  if (params.onlyConfirmed) q.set("onlyConfirmed", "true");
  if (params.onlyValidated) q.set("onlyValidated", "true");
  const s = q.toString();
  return apiFetch<DailySummaryProject[]>(`/api/v1/cashflow/reports/daily-summary${s ? `?${s}` : ""}`);
}
