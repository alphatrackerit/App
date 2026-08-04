import { ApiRequestError, apiFetch } from "@/lib/api-client";
import { tokenStore } from "@/auth/token-store";
import { env } from "@/env";
import type { PagedResponse } from "@/api/projects";
import type { FacturaType } from "@/api/facturas";

// ───────────────────────────────────────────────────────────────────────
//  Proformas — base /api/v1/cashflow/proformas
//  Un nivel por encima de Factura: 1 proforma → N facturas (ProformaId).
// ───────────────────────────────────────────────────────────────────────

export type ProformaDto = {
  id: string;
  number: string;
  type: FacturaType;
  date: string | null;
  clientId: string | null;
  supplierId: string | null;
  companyId: string | null;
  societyId: string | null;
  projectId: string | null;
  taxBase: number | null;
  vat: number | null;
  total: number;
  paymentTerms: string | null;
  statusId: string | null;
  responsible: string | null;
  notes: string | null;
  documentPath: string | null;
  /** Suma de totales de facturas vinculadas — cuadre informativo. */
  invoiced: number;
};

export type ProformaRow = ProformaDto;

// Create/Update payload. `type` is immutable on update (ignored server-side there).
export type ProformaInput = {
  type: FacturaType;
  number: string;
  total: number;
  clientId?: string | null;
  supplierId?: string | null;
  date?: string | null;
  companyId?: string | null;
  societyId?: string | null;
  projectId?: string | null;
  taxBase?: number | null;
  vat?: number | null;
  paymentTerms?: string | null;
  statusId?: string | null;
  responsible?: string | null;
  notes?: string | null;
};

// One invoice generated from / linked to the proforma.
export type ProformaInvoiceDto = {
  id: string;
  number: string;
  invoiceDate: string | null;
  dueDate: string | null;
  total: number;
  statusId: string | null;
};

// Proforma totals + linked invoices. `pending` may be negative (over-invoiced) — informative only.
export type ProformaLinesDto = {
  proformaId: string;
  total: number;
  invoicedAmount: number;
  pending: number;
  invoices: ProformaInvoiceDto[];
};

export type SearchProformasParams = {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
  type?: FacturaType;
  clientId?: string;
  supplierId?: string;
  companyId?: string;
  projectId?: string;
};

function proformaQuery(params: Record<string, string | number | undefined | null>): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null || v === "") continue;
    q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export function searchProformas(params: SearchProformasParams = {}): Promise<PagedResponse<ProformaRow>> {
  return apiFetch<PagedResponse<ProformaRow>>(`/api/v1/cashflow/proformas${proformaQuery(params)}`);
}

export function getProforma(id: string): Promise<ProformaDto> {
  return apiFetch<ProformaDto>(`/api/v1/cashflow/proformas/${encodeURIComponent(id)}`);
}

export function getProformaLines(id: string): Promise<ProformaLinesDto> {
  return apiFetch<ProformaLinesDto>(`/api/v1/cashflow/proformas/${encodeURIComponent(id)}/lines`);
}

export function createProforma(input: ProformaInput): Promise<string> {
  return apiFetch<string>("/api/v1/cashflow/proformas", { method: "POST", body: JSON.stringify(input) });
}

export function updateProforma(id: string, input: ProformaInput): Promise<string> {
  return apiFetch<string>(`/api/v1/cashflow/proformas/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteProforma(id: string): Promise<void> {
  await apiFetch<void>(`/api/v1/cashflow/proformas/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// Generate one draft invoice per payment-terms milestone; returns the created invoice ids.
// NOT idempotent by design (auto + manual linking combine) — the UI must confirm re-runs.
export function generateInvoicesFromProforma(id: string): Promise<string[]> {
  return apiFetch<string[]>(`/api/v1/cashflow/proformas/${encodeURIComponent(id)}/generate-invoices`, {
    method: "POST",
  });
}

// Link (or unlink, when proformaId is null) an invoice to a proforma.
export async function linkInvoiceToProforma(input: {
  invoiceId: string;
  proformaId: string | null;
}): Promise<void> {
  await apiFetch<void>("/api/v1/cashflow/proformas/link-invoice", { method: "POST", body: JSON.stringify(input) });
}

// Presigned URL to open the proforma's attached document in a new tab.
export function getProformaDocumentUrl(id: string): Promise<{ url: string }> {
  return apiFetch<{ url: string }>(`/api/v1/cashflow/proformas/${encodeURIComponent(id)}/document`);
}

// Attach (or replace) the source document of an EXISTING proforma.
export function attachProformaDocument(id: string, file: File): Promise<{ documentPath: string }> {
  const form = new FormData();
  form.append("file", file);
  return apiFetch<{ documentPath: string }>(
    `/api/v1/cashflow/proformas/${encodeURIComponent(id)}/document`,
    { method: "POST", body: form },
  );
}

/**
 * Stream the proforma PDF and trigger a browser download named
 * `proforma-{number}.pdf`. apiFetch only returns parsed JSON, so we fetch the
 * blob directly here while replicating the same auth + tenant headers apiFetch
 * sets (same pattern as billing's downloadInvoicePdf).
 */
export async function downloadProformaPdf(id: string, number: string): Promise<void> {
  const accessToken = tokenStore.getAccessToken();
  if (!accessToken) {
    throw new ApiRequestError(401, "Not signed in");
  }

  const headers = new Headers({ Authorization: `Bearer ${accessToken}` });
  const tenant = tokenStore.getTenant() ?? env.defaultTenant;
  if (tenant) headers.set("tenant", tenant);

  const response = await fetch(`${env.apiBase}/api/v1/cashflow/proformas/${encodeURIComponent(id)}/pdf`, {
    headers,
  });

  if (!response.ok) {
    throw new ApiRequestError(response.status, `No se pudo descargar el PDF (${response.status})`);
  }

  const blob = await response.blob();
  const objectUrl = window.URL.createObjectURL(blob);
  try {
    const anchor = document.createElement("a");
    anchor.href = objectUrl;
    anchor.download = `proforma-${number}.pdf`;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
  } finally {
    window.URL.revokeObjectURL(objectUrl);
  }
}
