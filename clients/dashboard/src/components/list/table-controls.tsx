import { useMemo, useState } from "react";
import { ArrowDown, ArrowUp, ListFilter } from "lucide-react";
import { cn } from "@/lib/cn";
import { PAGE_SIZE_ALL } from "./entity-shell";

// ───────────────────────────────────────────────────────────────────────
//  Per-column sort + filter.
//
//  Both run in memory over the rows the table holds, so a server-paged table
//  has to hold the WHOLE dataset for them to mean anything: sorting page 1 of
//  31 by "pendiente" finds the largest of those 20 rows, not of the 600.
//
//  `useTableState` handles that. While a sort or column filter is active it
//  widens the fetch to the entire dataset (`state.fetch`), and `useTableRows`
//  slices the sorted result back down to the page the user is standing on —
//  so the pager keeps working and the numbers are the real ones.
//
//  Tables that already hold every row (a local array, no pager) don't need
//  any of that and use the plain `useTableControls`.
// ───────────────────────────────────────────────────────────────────────

export type ColValue = string | number | null | undefined;
export type SortState = { key: string; dir: "asc" | "desc" } | null;
export type Getters<T> = Record<string, (row: T) => ColValue>;

/** Nulls sink to the bottom in both directions; numbers compare numerically,
 *  everything else with a Spanish natural-order collation. */
function applyControls<T>(
  rows: T[],
  getters: Getters<T>,
  filters: Record<string, string>,
  sort: SortState,
): T[] {
  let out = rows;
  for (const [key, raw] of Object.entries(filters)) {
    const f = raw.trim().toLowerCase();
    if (!f) continue;
    const get = getters[key];
    if (!get) continue;
    out = out.filter((r) => String(get(r) ?? "").toLowerCase().includes(f));
  }
  if (sort) {
    const get = getters[sort.key];
    if (get) {
      const dir = sort.dir === "asc" ? 1 : -1;
      out = [...out].sort((a, b) => {
        const va = get(a);
        const vb = get(b);
        if (va == null && vb == null) return 0;
        if (va == null) return 1;
        if (vb == null) return -1;
        if (typeof va === "number" && typeof vb === "number") return (va - vb) * dir;
        return String(va).localeCompare(String(vb), "es", { numeric: true, sensitivity: "base" }) * dir;
      });
    }
  }
  return out;
}

function useSortAndFilterState() {
  const [sort, setSort] = useState<SortState>(null);
  const [filters, setFilters] = useState<Record<string, string>>({});

  const toggleSort = (key: string) =>
    setSort((s) => (s?.key !== key ? { key, dir: "asc" } : s.dir === "asc" ? { key, dir: "desc" } : null));
  const setFilter = (key: string, v: string) => setFilters((f) => ({ ...f, [key]: v }));
  const clearFilters = () => setFilters({});
  const hasActiveFilters = Object.values(filters).some((v) => v.trim() !== "");

  return { sort, filters, toggleSort, setFilter, clearFilters, hasActiveFilters };
}

// ─── Server-paged tables: useTableState (above the query) + useTableRows (below) ───

export type TableState = ReturnType<typeof useTableState>;

/**
 * Sort + column-filter + paging state for a server-paged table. Call it
 * **above** the data query: `state.fetch` carries the page params to send, and
 * widens to the whole dataset as soon as a sort or column filter is on.
 */
export function useTableState({
  pageSize: initialPageSize = 20,
  /** Ceiling for the widened fetch. Lower it on tables where pulling every row
   *  is genuinely expensive (e.g. one extra request per row). */
  fullPageSize = PAGE_SIZE_ALL,
}: { pageSize?: number; fullPageSize?: number } = {}) {
  const base = useSortAndFilterState();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSizeRaw] = useState(initialPageSize);

  /** True while sort/filter is on: the table then pages in memory over the
   *  full dataset instead of asking the server for one page at a time. */
  const clientPaged = base.sort !== null || base.hasActiveFilters;

  // Any change to sort/filter/page size invalidates the current page index.
  const toggleSort = (key: string) => {
    base.toggleSort(key);
    setPage(1);
  };
  const setFilter = (key: string, v: string) => {
    base.setFilter(key, v);
    setPage(1);
  };
  const clearFilters = () => {
    base.clearFilters();
    setPage(1);
  };
  const setPageSize = (n: number) => {
    setPageSizeRaw(n);
    setPage(1);
  };

  return {
    ...base,
    toggleSort,
    setFilter,
    clearFilters,
    clientPaged,
    page,
    pageSize,
    setPage,
    setPageSize,
    /** Page params for the server query — spread into both the query key and
     *  the request so widening triggers a refetch. */
    fetch: clientPaged
      ? { pageNumber: 1, pageSize: fullPageSize }
      : { pageNumber: page, pageSize },
  };
}

/**
 * Sorts + filters the loaded rows, then slices them back to the user's page.
 * `server` supplies the totals to trust while no sort/filter is active (the
 * server is doing the paging then).
 */
export function useTableRows<T>(
  rows: T[],
  getters: Getters<T>,
  state: TableState,
  server: { totalCount?: number | null; totalPages?: number | null } = {},
) {
  const { sort, filters, clientPaged, page, pageSize } = state;

  // getters is re-created every render (it closes over catalog lookups) and is
  // deliberately not a dependency — the column set is static per table.
  const processed = useMemo(
    () => applyControls(rows, getters, filters, sort),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [rows, filters, sort],
  );

  const totalCount = clientPaged ? processed.length : (server.totalCount ?? processed.length);
  const totalPages = clientPaged
    ? Math.max(1, Math.ceil(processed.length / pageSize))
    : (server.totalPages ?? 1);

  const paged = useMemo(
    () => (clientPaged ? processed.slice((page - 1) * pageSize, page * pageSize) : processed),
    [processed, clientPaged, page, pageSize],
  );

  return {
    rows: paged,
    totalCount,
    totalPages,
    hasPrev: page > 1,
    hasNext: page < totalPages,
  };
}

// ─── Tables that already hold every row ───

/** Sort + column filters for a table whose `rows` are already the complete
 *  dataset (local array, no pager). Server-paged tables use
 *  `useTableState` + `useTableRows` instead. */
export function useTableControls<T>(rows: T[], getters: Getters<T>) {
  const base = useSortAndFilterState();
  const { filters, sort } = base;
  // getters is deliberately not a dependency — see useTableRows.
  const processed = useMemo(
    () => applyControls(rows, getters, filters, sort),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [rows, filters, sort],
  );
  return { ...base, rows: processed };
}

/** Empty-state row shown inside the table when the column filters match nothing —
 *  keeps the header (and its filter popovers) mounted so the user can adjust or clear them. */
export function EntityFilterEmptyRow({ onClear }: { onClear: () => void }) {
  return (
    <div className="flex flex-col items-center gap-2 px-4 py-10 text-center">
      <p className="text-[13px] text-[var(--color-muted-foreground)]">
        Ningún registro coincide con los filtros de columna.
      </p>
      <button
        type="button"
        onClick={onClear}
        className="cursor-pointer rounded-lg border border-[var(--color-border)] px-3 py-1.5 text-[12.5px] font-medium text-[var(--color-foreground)] transition-colors hover:bg-[var(--color-muted)]"
      >
        Limpiar filtros
      </button>
    </div>
  );
}

/** Header cell with sort toggle (click the label) and a funnel filter popover. */
export function EntityColHeader({
  colKey,
  label,
  sort,
  filters,
  onSort,
  onFilter,
  align,
}: {
  colKey: string;
  label: string;
  sort: SortState;
  filters: Record<string, string>;
  onSort: (key: string) => void;
  onFilter: (key: string, v: string) => void;
  align?: "right";
}) {
  const [open, setOpen] = useState(false);
  const activeSort = sort?.key === colKey ? sort.dir : null;
  const filterVal = filters[colKey] ?? "";

  return (
    <span className={cn("relative flex min-w-0 items-center gap-0.5", align === "right" && "justify-end")}>
      <button
        type="button"
        onClick={() => onSort(colKey)}
        title={`Ordenar por ${label}`}
        className="flex min-w-0 cursor-pointer items-center gap-0.5 truncate uppercase transition-colors hover:text-[var(--color-foreground)]"
      >
        <span className="truncate">{label}</span>
        {activeSort === "asc" && <ArrowUp className="size-3 shrink-0 text-[var(--color-primary)]" />}
        {activeSort === "desc" && <ArrowDown className="size-3 shrink-0 text-[var(--color-primary)]" />}
      </button>
      <button
        type="button"
        aria-label={`Filtrar por ${label}`}
        onClick={() => setOpen((o) => !o)}
        className={cn(
          "grid size-5 shrink-0 cursor-pointer place-items-center rounded transition-colors hover:bg-[var(--color-muted)]",
          filterVal ? "text-[var(--color-primary)]" : "text-[oklch(from_var(--color-muted-foreground)_l_c_h_/_0.6)]",
        )}
      >
        <ListFilter className="size-3" />
      </button>
      {open && (
        <span className="absolute left-0 top-full z-20 mt-1 block w-44 rounded-lg border border-[var(--color-border)] bg-[var(--color-card)] p-1.5 shadow-md">
          <input
            value={filterVal}
            onChange={(e) => onFilter(colKey, e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Escape" || e.key === "Enter") setOpen(false);
            }}
            onBlur={() => setOpen(false)}
            placeholder={`Filtrar ${label.toLowerCase()}…`}
            autoFocus
            className="h-7 w-full rounded-md border border-[var(--color-input)] bg-transparent px-2 text-[12px] font-normal normal-case text-[var(--color-foreground)] outline-none focus:border-[var(--color-primary)]"
          />
        </span>
      )}
    </span>
  );
}
