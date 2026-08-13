import { ApiRequestError, apiFetch } from "@/lib/api-client";
import { tokenStore } from "@/auth/token-store";
import { env } from "@/env";
import type { PagedResponse } from "@/api/projects";

// ───────────────────────────────────────────────────────────────────────
//  Facturas (invoices) — base /api/v1/cashflow/invoices
// ───────────────────────────────────────────────────────────────────────

export type FacturaType = "Emitida" | "Recibida";

// An invoice row/detail. Emitida → linked to a Cliente; Recibida → to a Proveedor.
export type FacturaDto = {
  id: string;
  /** Nulo en facturas recibidas en borrador (aún sin la factura del proveedor). */
  number: string | null;
  dynamicsNumber: string | null;
  type: FacturaType;
  invoiceDate: string | null;
  dueDate: string | null;
  clientId: string | null;
  supplierId: string | null;
  companyId: string | null;
  societyId: string | null;
  projectId: string | null;
  taxBase: number | null;
  vat: number | null;
  total: number;
  paymentTerms: string | null;
  bank: string | null;
  statusId: string | null;
  verified: boolean;
  notes: string | null;
  documentPath: string | null;
  /** Cobrado (emitida) / pagado (recibida): suma de líneas validadas vinculadas. */
  collected: number;
  /** Proforma de la que procede / a la que está vinculada (trazabilidad, opcional). */
  proformaId: string | null;
  /** Estado VERI*FACTU (AEAT). No confundir con el check interno `verified` ("Verificada"). */
  verifactuStatus: import("@/api/verifactu").VerifactuStatus;
  /** Conceptos (detalle presentable del PDF). Vacío en la búsqueda; completo en el detalle. */
  items: FacturaItemDto[];
};

/** Un concepto de la factura. `amount` lo deriva el servidor (cantidad × precio). */
export type FacturaItemDto = {
  id: string;
  description: string;
  quantity: number;
  unitPrice: number;
  amount: number;
};

export type FacturaItemInput = {
  description: string;
  quantity: number;
  unitPrice: number;
};

// The list/detail shape is identical — alias for readability at call sites.
export type FacturaRow = FacturaDto;

// Create/Update payload. `type` is immutable on update (ignored server-side there).
export type FacturaInput = {
  type: FacturaType;
  /** Obligatorio en Emitidas; opcional (null) en Recibidas — borrador sin número. */
  number: string | null;
  total: number;
  clientId?: string | null;
  supplierId?: string | null;
  invoiceDate?: string | null;
  dueDate?: string | null;
  companyId?: string | null;
  societyId?: string | null;
  projectId?: string | null;
  taxBase?: number | null;
  vat?: number | null;
  paymentTerms?: string | null;
  bank?: string | null;
  statusId?: string | null;
  dynamicsNumber?: string | null;
  notes?: string | null;
  documentPath?: string | null;
  /** Conceptos: null/omitido = no tocar; lista (incluso vacía) = reemplazar. */
  items?: FacturaItemInput[] | null;
};

// A cash-flow line (income/payment) linked to an invoice.
// Carries every editable field so updates can round-trip without wiping data.
export type InvoiceLineDto = {
  id: string;
  kind: "Income" | "Payment";
  amount: number;
  date: string | null;
  percentage: number | null;
  description: string | null;
  projectId: string | null;
  statusId: string | null;
  supplierId: string | null;
  confirmed: boolean;
  validated: boolean;
};

// Invoice totals + its linked lines, with reconciliation amounts.
export type InvoiceLinesDto = {
  invoiceId: string;
  total: number;
  linkedAmount: number;
  validatedAmount: number;
  pending: number;
  lines: InvoiceLineDto[];
};

export type SearchFacturasParams = {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
  // Optional server-side filters (serialized only when set):
  type?: FacturaType;
  clientId?: string;
  supplierId?: string;
  companyId?: string;
  projectId?: string;
  verifactuStatus?: import("@/api/verifactu").VerifactuStatus;
};

function invoiceQuery(params: Record<string, string | number | undefined | null>): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null || v === "") continue;
    q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export function searchFacturas(params: SearchFacturasParams = {}): Promise<PagedResponse<FacturaRow>> {
  return apiFetch<PagedResponse<FacturaRow>>(`/api/v1/cashflow/invoices${invoiceQuery(params)}`);
}

export function getFactura(id: string): Promise<FacturaDto> {
  return apiFetch<FacturaDto>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}`);
}

export function getFacturaLines(id: string): Promise<InvoiceLinesDto> {
  return apiFetch<InvoiceLinesDto>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}/lines`);
}

export function createFactura(input: FacturaInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/invoices", { method: "POST", body: JSON.stringify(input) });
}

export function updateFactura(id: string, input: FacturaInput): Promise<string> {
  return apiFetch<string>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteFactura(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// Link (or unlink, when invoiceId is null) a cash-flow line to this invoice.
export async function linkLine(input: {
  lineId: string;
  kind: "Income" | "Payment";
  invoiceId: string | null;
}): Promise<void> {
  await apiFetch<void>("/api/v1/cashflow/invoices/link-line", { method: "POST", body: JSON.stringify(input) });
}

// ─── Assisted linking (pasada asistida) ───

// One proposed invoice ↔ line pair, with a human-readable reason.
export type LinkSuggestionDto = {
  invoiceId: string;
  invoiceNumber: string | null;
  invoiceType: FacturaType;
  invoiceTotal: number;
  counterpartyName: string | null;
  lineId: string;
  kind: "Income" | "Payment";
  lineAmount: number;
  lineDate: string | null;
  lineDescription: string | null;
  projectName: string | null;
  reason: string;
};

export type LinkSuggestionsDto = {
  suggestions: LinkSuggestionDto[];
  invoicesWithoutLines: number;
  unlinkedLines: number;
  ambiguousInvoices: number;
};

export type LinkPair = { invoiceId: string; lineId: string; kind: "Income" | "Payment" };

export function getLinkSuggestions(type?: FacturaType): Promise<LinkSuggestionsDto> {
  const q = type ? `?type=${encodeURIComponent(type)}` : "";
  return apiFetch<LinkSuggestionsDto>(`/api/v1/cashflow/invoices/link-suggestions${q}`);
}

export function applyLinkSuggestions(pairs: LinkPair[]): Promise<{ linked: number; skipped: number }> {
  return apiFetch<{ linked: number; skipped: number }>("/api/v1/cashflow/invoices/link-suggestions/apply", {
    method: "POST",
    body: JSON.stringify({ pairs }),
  });
}

// Generate the milestone lines for an invoice; returns the created line ids.
export function generateMilestones(id: string, projectId?: string | null): Promise<string[]> {
  return apiFetch<string[]>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}/generate-milestones`, {
    method: "POST",
    body: JSON.stringify({ projectId: projectId ?? null }),
  });
}

/**
 * Descarga el PDF presentable de la factura (con QR + leyenda AEAT si está registrada en
 * VeriFactu). apiFetch solo devuelve JSON, así que pedimos el blob directamente con los mismos
 * headers de auth + tenant.
 */
export async function downloadFacturaPdf(invoiceId: string, invoiceNumber: string | null): Promise<void> {
  const accessToken = tokenStore.getAccessToken();
  if (!accessToken) {
    throw new ApiRequestError(401, "Not signed in");
  }

  const headers = new Headers({ Authorization: `Bearer ${accessToken}` });
  const tenant = tokenStore.getTenant() ?? env.defaultTenant;
  if (tenant) headers.set("tenant", tenant);

  const response = await fetch(
    `${env.apiBase}/api/v1/cashflow/invoices/${encodeURIComponent(invoiceId)}/pdf`,
    { headers },
  );

  if (!response.ok) {
    throw new ApiRequestError(response.status, `No se pudo descargar el PDF (${response.status})`);
  }

  const blob = await response.blob();
  const objectUrl = window.URL.createObjectURL(blob);
  try {
    const anchor = document.createElement("a");
    anchor.href = objectUrl;
    anchor.download = `factura-${invoiceNumber ?? "sin-numero"}.pdf`;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
  } finally {
    window.URL.revokeObjectURL(objectUrl);
  }
}

export async function verifyFactura(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}/verify`, { method: "POST" });
}

// ─── Reporte de facturas (vencidas / por vencer / cobradas / por cobrar) ───

export type InvoiceReportScope = "Todas" | "Vencidas" | "PorVencer" | "Cobradas" | "PorCobrar";
export type InvoiceReportStatus = "Settled" | "Overdue" | "Upcoming" | "NoDueDate";

export type InvoiceReportRow = {
  id: string;
  number: string | null;
  type: FacturaType;
  counterpartyName: string | null;
  companyName: string | null;
  invoiceDate: string | null;
  dueDate: string | null;
  total: number;
  collected: number;
  pending: number;
  daysOverdue: number | null; // positivo = días vencida; negativo = días hasta vencer
  status: InvoiceReportStatus;
};

export type InvoiceReportBucket = { count: number; amount: number };

export type InvoiceReport = {
  generatedAt: string;
  totalCount: number;
  totalAmount: number;
  settled: InvoiceReportBucket;
  pending: InvoiceReportBucket;
  overdue: InvoiceReportBucket;
  upcoming: InvoiceReportBucket;
  rows: InvoiceReportRow[];
};

export type InvoiceReportParams = {
  scope?: InvoiceReportScope;
  type?: FacturaType;
  clientId?: string;
  supplierId?: string;
  companyId?: string;
  from?: string; // yyyy-mm-dd
  to?: string;
  dueWithinDays?: number;
};

export function getInvoicesReport(params: InvoiceReportParams = {}): Promise<InvoiceReport> {
  return apiFetch<InvoiceReport>(`/api/v1/cashflow/reports/invoices${invoiceQuery(params)}`);
}

// ─── Extracción IA desde documento ───

// AI-proposed invoice data extracted from an uploaded PDF/image. Everything is a suggestion the
// user reviews; documentPath is the stored file to attach on create.
export type InvoiceExtractionDto = {
  type: FacturaType | null;
  number: string | null;
  dynamicsNumber: string | null;
  invoiceDate: string | null;
  dueDate: string | null;
  taxBase: number | null;
  vat: number | null;
  total: number | null;
  paymentTerms: string | null;
  counterpartyName: string | null;
  matchedClientId: string | null;
  matchedSupplierId: string | null;
  bank: string | null;
  notes: string | null;
  documentPath: string;
};

export function extractInvoice(file: File): Promise<InvoiceExtractionDto> {
  const form = new FormData();
  form.append("file", file);
  // apiFetch only auto-sets Content-Type for string bodies — FormData keeps its own boundary.
  return apiFetch<InvoiceExtractionDto>("/api/v1/cashflow/invoices/extract", {
    method: "POST",
    body: form,
  });
}

// Presigned URL to open the invoice's attached document in a new tab.
export function getInvoiceDocumentUrl(id: string): Promise<{ url: string }> {
  return apiFetch<{ url: string }>(`/api/v1/cashflow/invoices/${encodeURIComponent(id)}/document`);
}

// Attach (or replace) the digitalized document of an EXISTING invoice.
export function attachInvoiceDocument(id: string, file: File): Promise<{ documentPath: string }> {
  const form = new FormData();
  form.append("file", file);
  return apiFetch<{ documentPath: string }>(
    `/api/v1/cashflow/invoices/${encodeURIComponent(id)}/document`,
    { method: "POST", body: form },
  );
}
