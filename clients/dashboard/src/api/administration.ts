import { apiFetch } from "@/lib/api-client";
import type { PagedResponse } from "@/api/projects";

export type Lookup = { id: string; name: string; code: string | null };
export type LookupInput = { name: string; code?: string | null };

export type SearchLookupParams = {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
  // Optional server-side filters (serialized only when set):
  type?: string;        // statuses / prefixes
  onlyActive?: boolean; // prefixes
  clientId?: string;    // societies
};

function query(params: SearchLookupParams): string {
  const q = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null || v === "") continue;
    q.set(k, String(v));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

export type CatalogApi = {
  search: (params?: SearchLookupParams) => Promise<PagedResponse<Lookup>>;
  create: (input: LookupInput) => Promise<string>;
  update: (id: string, input: LookupInput) => Promise<string>;
  remove: (id: string) => Promise<void>;
};

function catalogApi(base: string): CatalogApi {
  return {
    search: (params = {}) => apiFetch<PagedResponse<Lookup>>(`/api/v1/cashflow${base}${query(params)}`),
    create: (input) => apiFetch<string>(`/api/v1/cashflow${base}`, { method: "POST", body: JSON.stringify(input) }),
    update: (id, input) =>
      apiFetch<string>(`/api/v1/cashflow${base}/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(input) }),
    remove: async (id) => {
      await apiFetch<void>(`/api/v1/cashflow${base}/${encodeURIComponent(id)}`, { method: "DELETE" });
    },
  };
}

export const clientsApi = catalogApi("/clients");
export const suppliersApi = catalogApi("/suppliers");
export const countriesApi = catalogApi("/countries");
export const statusesApi = catalogApi("/statuses");
export const companiesApi = catalogApi("/companies");

// ───────────────────────────────────────────────────────────────────────
//  Rich catalogs (extra fields beyond name+code) — spec §2 maestros
// ───────────────────────────────────────────────────────────────────────

export type RichCatalogApi<TRow, TInput> = {
  search: (params?: SearchLookupParams) => Promise<PagedResponse<TRow>>;
  create: (input: TInput) => Promise<string>;
  update: (id: string, input: TInput) => Promise<string>;
  remove: (id: string) => Promise<void>;
};

function richCatalog<TRow, TInput>(base: string): RichCatalogApi<TRow, TInput> {
  return {
    search: (params = {}) => apiFetch<PagedResponse<TRow>>(`/api/v1/cashflow${base}${query(params)}`),
    create: (input) => apiFetch<string>(`/api/v1/cashflow${base}`, { method: "POST", body: JSON.stringify(input) }),
    update: (id, input) =>
      apiFetch<string>(`/api/v1/cashflow${base}/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(input) }),
    remove: async (id) => {
      await apiFetch<void>(`/api/v1/cashflow${base}/${encodeURIComponent(id)}`, { method: "DELETE" });
    },
  };
}

// Supplier — the color/priority drive the cash-flow calendar.
export type SupplierRow = Lookup & { colorHex: string | null; visualPriority: number };
export type SupplierInput = LookupInput & { colorHex?: string | null; visualPriority?: number };
export const supplierCatalog = richCatalog<SupplierRow, SupplierInput>("/suppliers");

// Status — polymorphic by Type (PROYECTO | INGRESO | PAGO) + color.
export type StatusType = "Proyecto" | "Ingreso" | "Pago";
export type StatusRow = Lookup & { type: StatusType | null; colorHex: string | null };
export type StatusInput = LookupInput & { type?: StatusType | null; colorHex?: string | null };
export const statusCatalog = richCatalog<StatusRow, StatusInput>("/statuses");

// Society — a legal entity belonging to a Client.
export type SocietyRow = {
  id: string;
  name: string;
  taxId: string | null;
  address: string | null;
  postalCode: string | null;
  city: string | null;
  country: string | null;
  clientId: string | null;
};
export type SocietyInput = {
  name: string;
  taxId?: string | null;
  address?: string | null;
  postalCode?: string | null;
  city?: string | null;
  country?: string | null;
  clientId?: string | null;
};
export const societiesApi = richCatalog<SocietyRow, SocietyInput>("/societies");

// Prefix — hierarchical GRUPO → CATEGORIA project categorization.
export type PrefixType = "Grupo" | "Categoria";
export type PrefixRow = {
  id: string;
  name: string;
  description: string | null;
  prefixGroupId: string | null;
  type: PrefixType;
  isActive: boolean;
};
export type PrefixInput = {
  name: string;
  description?: string | null;
  prefixGroupId?: string | null;
  type: PrefixType;
  isActive: boolean;
};
export const prefixesApi = richCatalog<PrefixRow, PrefixInput>("/prefixes");
