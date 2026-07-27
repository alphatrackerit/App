import { useMemo, useState } from "react";
import { ArrowDown, ArrowUp, ListFilter } from "lucide-react";
import { cn } from "@/lib/cn";

// ───────────────────────────────────────────────────────────────────────
//  Per-column sort + filter over the loaded rows (client-side). With the
//  pager's "Todos" page size this effectively covers the whole dataset.
// ───────────────────────────────────────────────────────────────────────

export type ColValue = string | number | null | undefined;
export type SortState = { key: string; dir: "asc" | "desc" } | null;

export function useTableControls<T>(rows: T[], getters: Record<string, (row: T) => ColValue>) {
  const [sort, setSort] = useState<SortState>(null);
  const [filters, setFilters] = useState<Record<string, string>>({});

  // getters is typically re-created per render (it closes over catalog lookups) —
  // cheap enough: filtering/sorting one page of rows.
  const processed = useMemo(() => {
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
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [rows, filters, sort]);

  const toggleSort = (key: string) =>
    setSort((s) => (s?.key !== key ? { key, dir: "asc" } : s.dir === "asc" ? { key, dir: "desc" } : null));
  const setFilter = (key: string, v: string) => setFilters((f) => ({ ...f, [key]: v }));
  const clearFilters = () => setFilters({});
  const hasActiveFilters = Object.values(filters).some((v) => v.trim() !== "");

  return { rows: processed, sort, filters, toggleSort, setFilter, clearFilters, hasActiveFilters };
}

/** Empty-state row shown inside the table when the column filters match nothing —
 *  keeps the header (and its filter popovers) mounted so the user can adjust or clear them. */
export function EntityFilterEmptyRow({ onClear }: { onClear: () => void }) {
  return (
    <div className="flex flex-col items-center gap-2 px-4 py-10 text-center">
      <p className="text-[13px] text-[var(--color-muted-foreground)]">
        Ningún registro de esta página coincide con los filtros de columna.
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
