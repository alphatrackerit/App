import { useEffect, useMemo, useState } from "react";
import { keepPreviousData, useQueries, useQuery } from "@tanstack/react-query";
import { ChevronDown, ScrollText, Search } from "lucide-react";
import { getAuditById, listAudits, type AuditDetailDto, type AuditSummaryDto } from "@/api/audits";
import { searchIncomes, searchPayments, searchProjects } from "@/api/projects";
import { clientsApi, companiesApi, countriesApi, societiesApi, statusesApi, suppliersApi } from "@/api/administration";
import {
  EntityColHeader,
  EntityFilterEmptyRow,
  EntityEmpty,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityPageHeader,
  EntityPager,
  EntitySearch,
  EntityStatusBadge,
  useTableControls,
} from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

const PAGE_SIZE = 20;
const COLS = "grid-cols-[140px_minmax(0,1fr)_140px_120px_minmax(0,1fr)_36px]";
// Por encima de esto no se precargan los detalles (una petición por fila) — usa la búsqueda global.
const DETAIL_PREFETCH_LIMIT = 200;

// Operación → etiqueta y tono. Claves = EntityOperation del backend.
const OPERATIONS: Record<string, { label: string; tone: "success" | "info" | "danger" | "warning" | "default" }> = {
  Insert: { label: "Alta", tone: "success" },
  Update: { label: "Modificación", tone: "info" },
  Delete: { label: "Borrado", tone: "danger" },
  SoftDelete: { label: "Borrado", tone: "danger" },
  Restore: { label: "Restaurado", tone: "warning" },
};

// El payload de un evento EntityChange (Modules.Auditing.Contracts.EntityChangeEventPayload).
type EntityChangePayload = {
  dbContext: string;
  table: string;
  entityName: string;
  key: string;
  operation: string;
  changes: { name: string; oldValue: unknown; newValue: unknown; isSensitive: boolean }[];
};

type LogRow = AuditSummaryDto & { payload: EntityChangePayload | null };

/** id (GUID) → nombre legible, alimentado por proyectos + catálogos + líneas de caja. */
type NameIndex = Map<string, string>;

function payloadOf(detail: AuditDetailDto | undefined): EntityChangePayload | null {
  const p = detail?.payload as EntityChangePayload | undefined;
  return p && p.table ? p : null;
}

const GUID_RE = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

function keyGuid(key: string): string | null {
  const m = key.match(/Id:([0-9a-f-]{36})/i);
  return m ? m[1].toLowerCase() : null;
}

// El nombre del registro afectado: para Ingresos/Pagos, el proyecto al que pertenece
// (resuelto vía el índice de nombres); para el resto, el campo Nombre/Descripción del diff.
function recordLabel(p: EntityChangePayload | null, names: NameIndex): string {
  if (!p) return "";
  const own = keyGuid(p.key);
  if (own && names.has(own)) return names.get(own)!;
  const named = p.changes?.find((c) => /^(nombre|name|descripcion|description|numero|number|title|titulo)$/i.test(c.name));
  const v = named?.newValue ?? named?.oldValue;
  return v == null || v === "" ? "" : String(v);
}

function fmtVal(v: unknown, names: NameIndex): string {
  if (v === null || v === undefined || v === "") return "—";
  if (typeof v === "boolean") return v ? "Sí" : "No";
  const s = String(v);
  if (GUID_RE.test(s)) return names.get(s.toLowerCase()) ?? s;
  if (/^\d{4}-\d{2}-\d{2}T/.test(s)) return formatDate(s);
  return s.length > 80 ? `${s.slice(0, 80)}…` : s;
}

function fmtWhen(iso: string): string {
  const d = new Date(iso);
  return `${d.toLocaleDateString("es-ES")} ${d.toLocaleTimeString("es-ES", { hour: "2-digit", minute: "2-digit" })}`;
}

// Índice global id→nombre: proyectos, catálogos y líneas de caja (ingreso/pago → su proyecto).
function useNameIndex(): NameIndex {
  const projectsQ = useQuery({
    queryKey: ["projects", "all-names"],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    staleTime: 5 * 60_000,
  });
  const incomesQ = useQuery({
    queryKey: ["incomes", "all-names"],
    queryFn: () => searchIncomes({ pageSize: 10000 }),
    staleTime: 5 * 60_000,
  });
  const paymentsQ = useQuery({
    queryKey: ["payments", "all-names"],
    queryFn: () => searchPayments({ pageSize: 10000 }),
    staleTime: 5 * 60_000,
  });
  const cat = (key: string, api: { search: (p?: { pageSize?: number }) => Promise<{ items: { id: string; name: string }[] }> }) =>
    // eslint-disable-next-line react-hooks/rules-of-hooks
    useQuery({
      queryKey: ["administration", key, "options"],
      queryFn: () => api.search({ pageSize: 10000 }),
      staleTime: 5 * 60_000,
    });
  const statusesQ = cat("statuses", statusesApi);
  const suppliersQ = cat("suppliers", suppliersApi);
  const clientsQ = cat("clients", clientsApi);
  const companiesQ = cat("companies", companiesApi);
  const countriesQ = cat("countries", countriesApi);
  const societiesQ = cat("societies", societiesApi);

  return useMemo(() => {
    const m: NameIndex = new Map();
    const projName = new Map<string, string>();
    for (const p of projectsQ.data?.items ?? []) {
      projName.set(p.id.toLowerCase(), p.name);
      m.set(p.id.toLowerCase(), p.name);
    }
    // Un ingreso/pago se etiqueta con el proyecto al que pertenece (+ descripción).
    for (const i of incomesQ.data?.items ?? []) {
      const pn = i.projectId ? projName.get(i.projectId.toLowerCase()) : undefined;
      m.set(i.id.toLowerCase(), [pn, i.description].filter(Boolean).join(" · ") || i.id);
    }
    for (const p of paymentsQ.data?.items ?? []) {
      const pn = p.projectId ? projName.get(p.projectId.toLowerCase()) : undefined;
      m.set(p.id.toLowerCase(), [pn, p.description].filter(Boolean).join(" · ") || p.id);
    }
    for (const q of [statusesQ, suppliersQ, clientsQ, companiesQ, countriesQ, societiesQ]) {
      for (const it of q.data?.items ?? []) m.set(it.id.toLowerCase(), it.name);
    }
    return m;
  }, [projectsQ.data, incomesQ.data, paymentsQ.data, statusesQ.data, suppliersQ.data, clientsQ.data, companiesQ.data, countriesQ.data, societiesQ.data]);
}

function RowDetail({ row, names }: { row: LogRow; names: NameIndex }) {
  const p = row.payload;
  if (!p) {
    return <p className="px-4 py-3 text-[12.5px] text-[var(--color-muted-foreground)]">Sin detalle de campos.</p>;
  }
  const changes = p.changes ?? [];
  return (
    <div className="border-t border-dashed border-[var(--color-border)] bg-[var(--color-muted)]/25 px-4 py-3">
      <p className="mb-2 flex flex-wrap items-center gap-2 text-[12.5px] text-[var(--color-muted-foreground)]">
        Registro{" "}
        {recordLabel(p, names) ? (
          <strong className="text-[var(--color-foreground)]">{recordLabel(p, names)}</strong>
        ) : null}{" "}
        <code className="font-mono text-[11px]">{p.key}</code>
      </p>
      {changes.length === 0 ? (
        <p className="text-[12.5px] text-[var(--color-muted-foreground)]">Sin cambios de campos registrados.</p>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-[12.5px]">
            <thead>
              <tr className="text-left text-[11px] uppercase tracking-wide text-[var(--color-muted-foreground)]">
                <th className="py-1 pr-4 font-semibold">Campo</th>
                <th className="py-1 pr-4 font-semibold">Antes</th>
                <th className="py-1 font-semibold">Después</th>
              </tr>
            </thead>
            <tbody>
              {changes.map((c) => (
                <tr key={c.name} className="border-t border-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
                  <td className="py-1 pr-4 font-medium text-[var(--color-foreground)]">{c.name}</td>
                  <td className="py-1 pr-4 text-[var(--color-muted-foreground)]">{c.isSensitive ? "****" : fmtVal(c.oldValue, names)}</td>
                  <td className="py-1 text-[var(--color-foreground)]">{c.isSensitive ? "****" : fmtVal(c.newValue, names)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

export function LogPage() {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE);
  const [expanded, setExpanded] = useState<string | null>(null);

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPageNumber(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search]);

  // Solo cambios de entidades del módulo Cashflow (proyectos + administración), de todos los usuarios.
  const q = useQuery({
    queryKey: ["audits", "cashflow-log", { search: debouncedSearch, pageNumber, pageSize }],
    queryFn: () =>
      listAudits({
        eventType: "EntityChange",
        source: "CashflowDbContext",
        search: debouncedSearch || undefined,
        pageNumber,
        pageSize,
      }),
    placeholderData: keepPreviousData,
  });

  const data = q.data;
  const summaries: AuditSummaryDto[] = data?.items ?? [];

  // Detalles de la página visible (uno por fila, cacheados para siempre — un audit es inmutable).
  const canPrefetch = summaries.length <= DETAIL_PREFETCH_LIMIT;
  const detailQs = useQueries({
    queries: (canPrefetch ? summaries : []).map((s) => ({
      queryKey: ["audits", "detail", s.id],
      queryFn: () => getAuditById(s.id),
      staleTime: Infinity,
    })),
  });
  const detailById = useMemo(() => {
    const m = new Map<string, AuditDetailDto>();
    for (const dq of detailQs) if (dq.data) m.set(dq.data.id, dq.data);
    return m;
  }, [detailQs]);

  const names = useNameIndex();

  const rows: LogRow[] = useMemo(
    () => summaries.map((s) => ({ ...s, payload: payloadOf(detailById.get(s.id)) })),
    [summaries, detailById],
  );

  const ctl = useTableControls<LogRow>(rows, {
    fecha: (r) => r.occurredAtUtc,
    usuario: (r) => r.userName || r.userId || "Sistema",
    entidad: (r) => r.payload?.table ?? null,
    operacion: (r) => (r.payload ? (OPERATIONS[r.payload.operation]?.label ?? r.payload.operation) : null),
    registro: (r) => (r.payload ? recordLabel(r.payload, names) || r.payload.key : null),
  });
  const items = ctl.rows;
  const searchActive = debouncedSearch.length > 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={ScrollText}
        title="Log de cambios"
        total={data?.totalCount ?? null}
        unit="cambio"
        description="Altas, modificaciones y borrados de Proyectos y Administración, de todos los usuarios. Ordena y filtra por columna, o usa la búsqueda global (rastrea también el contenido del cambio)."
      />

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar en todo (usuario, proyecto, importes…)…" />

      {!canPrefetch && (
        <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-2 text-[12px] text-[var(--color-muted-foreground)]">
          Con más de {DETAIL_PREFETCH_LIMIT} filas cargadas no se rellenan Entidad/Operación/Registro (una petición por fila). Usa
          la búsqueda global o un tamaño de página menor.
        </p>
      )}

      {q.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 && !ctl.hasActiveFilters ? (
        <EntityEmpty
          icon={searchActive ? Search : ScrollText}
          title={searchActive ? "Sin resultados" : "Aún no hay cambios registrados"}
          body={
            searchActive
              ? `Nada coincide con "${debouncedSearch}".`
              : "Cuando alguien cree, edite o borre registros del módulo aparecerán aquí."
          }
        />
      ) : (
        <div>
          <EntityListCard>
            <EntityListHeader className={COLS}>
              {(
                [
                  ["fecha", "Fecha"],
                  ["usuario", "Usuario"],
                  ["entidad", "Entidad"],
                  ["operacion", "Operación"],
                  ["registro", "Registro"],
                ] as const
              ).map(([key, label]) => (
                <EntityColHeader
                  key={key}
                  colKey={key}
                  label={label}
                  sort={ctl.sort}
                  filters={ctl.filters}
                  onSort={ctl.toggleSort}
                  onFilter={ctl.setFilter}
                />
              ))}
              <span />
            </EntityListHeader>
            {items.length === 0 && <EntityFilterEmptyRow onClear={ctl.clearFilters} />}
            {items.map((it, i) => {
              const op = it.payload ? (OPERATIONS[it.payload.operation] ?? { label: it.payload.operation, tone: "default" as const }) : null;
              return (
                <div key={it.id}>
                  <EntityListRow
                    className={COLS}
                    isLast={i === items.length - 1 && expanded !== it.id}
                    onClick={() => setExpanded((e) => (e === it.id ? null : it.id))}
                  >
                    <span className="text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">{fmtWhen(it.occurredAtUtc)}</span>
                    <span className="truncate text-[13.5px] font-medium text-[var(--color-foreground)]">
                      {it.userName || it.userId || "Sistema"}
                    </span>
                    <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">{it.payload?.table ?? "…"}</span>
                    <span>{op ? <EntityStatusBadge tone={op.tone}>{op.label}</EntityStatusBadge> : "…"}</span>
                    <span className="truncate text-[13px] text-[var(--color-muted-foreground)]" title={it.payload?.key}>
                      {it.payload ? recordLabel(it.payload, names) || it.payload.key : "…"}
                    </span>
                    <span className="flex justify-end">
                      <ChevronDown
                        className={`size-4 text-[var(--color-muted-foreground)] transition-transform ${expanded === it.id ? "rotate-180" : ""}`}
                      />
                    </span>
                  </EntityListRow>
                  {expanded === it.id && <RowDetail row={it} names={names} />}
                </div>
              );
            })}
          </EntityListCard>

          <EntityPager
            page={data?.pageNumber ?? 1}
            totalPages={data?.totalPages ?? 1}
            hasPrev={(data?.pageNumber ?? 1) > 1}
            hasNext={(data?.pageNumber ?? 1) < (data?.totalPages ?? 1)}
            onPrev={() => setPageNumber((p) => Math.max(1, p - 1))}
            onNext={() => setPageNumber((p) => p + 1)}
            pageSize={pageSize}
            onPageSizeChange={(s) => {
              setPageSize(s);
              setPageNumber(1);
            }}
          />
        </div>
      )}

      {q.isError && (
        <div
          role="alert"
          className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]"
        >
          {describe(q.error)}
        </div>
      )}
    </div>
  );
}
