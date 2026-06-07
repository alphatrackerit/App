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
    search: (params = {}) => apiFetch<PagedResponse<Lookup>>(`/api/v1${base}${query(params)}`),
    create: (input) => apiFetch<string>(`/api/v1${base}`, { method: "POST", body: JSON.stringify(input) }),
    update: (id, input) =>
      apiFetch<string>(`/api/v1${base}/${encodeURIComponent(id)}`, { method: "PUT", body: JSON.stringify(input) }),
    remove: async (id) => {
      await apiFetch<void>(`/api/v1${base}/${encodeURIComponent(id)}`, { method: "DELETE" });
    },
  };
}

export const clientsApi = catalogApi("/clients");
export const suppliersApi = catalogApi("/suppliers");
export const countriesApi = catalogApi("/countries");
export const statusesApi = catalogApi("/statuses");
export const companiesApi = catalogApi("/companies");
