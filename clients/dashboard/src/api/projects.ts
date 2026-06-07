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
  countryId?: string | null;
  companyId?: string | null;
  statusId?: string | null;
};

export function searchProjects(params: PagedParams = {}): Promise<PagedResponse<ProjectDto>> {
  return apiFetch<PagedResponse<ProjectDto>>(`/api/v1/projects${pagedQuery(params)}`);
}

export function getProject(id: string): Promise<ProjectDto> {
  return apiFetch<ProjectDto>(`/api/v1/projects/${encodeURIComponent(id)}`);
}

export function createProject(input: ProjectInput): Promise<string> {
  return apiFetch<string>("/api/v1/projects", { method: "POST", body: JSON.stringify(input) });
}

export function updateProject(id: string, input: ProjectInput): Promise<string> {
  return apiFetch<string>(`/api/v1/projects/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteProject(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/projects/${encodeURIComponent(id)}`, { method: "DELETE" });
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
  confirmed: boolean;
  validated: boolean;
};

export type IncomeInput = {
  amount: number;
  description?: string | null;
  date?: string | null;
  percentage?: number | null;
  projectId?: string | null;
  confirmed?: boolean;
};

export function searchIncomes(
  params: PagedParams & { projectId?: string } = {},
): Promise<PagedResponse<IncomeDto>> {
  return apiFetch<PagedResponse<IncomeDto>>(`/api/v1/incomes${pagedQuery(params)}`);
}

export function createIncome(input: IncomeInput): Promise<string> {
  return apiFetch<string>("/api/v1/incomes", { method: "POST", body: JSON.stringify(input) });
}

export function updateIncome(id: string, input: IncomeInput): Promise<string> {
  return apiFetch<string>(`/api/v1/incomes/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteIncome(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/incomes/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export async function confirmIncome(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/incomes/${encodeURIComponent(id)}/confirm`, { method: "POST" });
}

export async function validateIncome(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/incomes/${encodeURIComponent(id)}/validate`, { method: "POST" });
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
  confirmed: boolean;
  validated: boolean;
};

export type PaymentInput = {
  amount: number;
  description?: string | null;
  date?: string | null;
  percentage?: number | null;
  projectId?: string | null;
};

export function searchPayments(
  params: PagedParams & { projectId?: string } = {},
): Promise<PagedResponse<PaymentDto>> {
  return apiFetch<PagedResponse<PaymentDto>>(`/api/v1/payments${pagedQuery(params)}`);
}

export function createPayment(input: PaymentInput): Promise<string> {
  return apiFetch<string>("/api/v1/payments", { method: "POST", body: JSON.stringify(input) });
}

export function updatePayment(id: string, input: PaymentInput): Promise<string> {
  return apiFetch<string>(`/api/v1/payments/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deletePayment(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/payments/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export async function confirmPayment(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/payments/${encodeURIComponent(id)}/confirm`, { method: "POST" });
}

export async function validatePayment(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/payments/${encodeURIComponent(id)}/validate`, { method: "POST" });
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
  return apiFetch<PagedResponse<NoteDto>>(`/api/v1/notes${pagedQuery(params)}`);
}

export function createNote(input: NoteInput): Promise<string> {
  return apiFetch<string>("/api/v1/notes", { method: "POST", body: JSON.stringify(input) });
}

export function updateNote(id: string, input: NoteInput): Promise<string> {
  return apiFetch<string>(`/api/v1/notes/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteNote(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/notes/${encodeURIComponent(id)}`, { method: "DELETE" });
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
  return apiFetch<Cashflow>("/api/v1/cashflow");
}
