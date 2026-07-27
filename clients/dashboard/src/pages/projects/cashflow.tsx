import { Fragment, useEffect, useMemo, useRef, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { CalendarClock, ChevronLeft, ChevronRight, Eye, ListFilter, Search, X } from "lucide-react";
import { getDailySummary, type DailySummaryProject } from "@/api/projects";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogBody,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { EntityStatusBadge, PageHero } from "@/components/list";
import { describe } from "@/lib/list-helpers";

const MONTHS = [
  "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
  "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre",
];
const MONTH_NUMS = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
const DAYS = Array.from({ length: 31 }, (_, i) => i + 1);
const TRANSPARENT = "#ffffff00";

type StatusFilter = "all" | "confirmed" | "validated";

function money(n: number): string {
  return new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n);
}
function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
}
function isWeekend(year: number, month: number, day: number): boolean {
  const wd = new Date(year, month - 1, day).getDay();
  return wd === 0 || wd === 6;
}
function ymd(iso: string): { y: number; m: number; d: number } {
  // Local getters; backend serializes "2026-07-14T00:00:00" (no TZ offset).
  const dt = new Date(iso);
  return { y: dt.getFullYear(), m: dt.getMonth() + 1, d: dt.getDate() };
}
// Faint tint from a raw supplier color; the alpha keeps it legible in light & dark.
// Accepts #rrggbb and CSS color functions (rgb/rgba/hsl/hsla/oklch/oklab) — legacy data stores
// colors as rgba(). `#ffffff00` (the TRANSPARENT sentinel, 8 hex) intentionally doesn't match.
function hexTint(color: string, alpha: number): string | undefined {
  return /^#[0-9a-fA-F]{6}$/.test(color) || /^(rgba?|hsla?|oklch|oklab|color)\(/i.test(color)
    ? `oklch(from ${color} l c h / ${alpha})`
    : undefined;
}

type CellDetail = {
  id: string;
  amount: number;
  percentage: number | null;
  projectId: string;
  projectName: string;
  supplierName: string | null;
  description: string | null;
  confirmed: boolean;
  validated: boolean;
  status: string;
};
type DayCell = { inc: number; pay: number; color: string; priority: number; incList: CellDetail[]; payList: CellDetail[] };
type DetailState = { title: string; date: string; kind: "inc" | "pay"; entries: CellDetail[] } | null;

export function FlujoDeCajaPage() {
  const now = new Date();
  const [searchParams, setSearchParams] = useSearchParams();
  // "Ver reporte" desde el detalle de proyecto llega con ?proyecto=<id> → informe de ese proyecto solamente.
  const soloProjectId = searchParams.get("proyecto");
  const [year, setYear] = useState<number>(now.getFullYear());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [excluded, setExcluded] = useState<Set<string>>(new Set());
  const [projectsOpen, setProjectsOpen] = useState(false);
  const [detail, setDetail] = useState<DetailState>(null);

  // The server computes totals + predominant-supplier color; status filter is a server param
  // so both stay accurate, while the project include/exclude is applied client-side.
  const q = useQuery({
    queryKey: ["cashflow", "summary", statusFilter, soloProjectId],
    queryFn: () =>
      getDailySummary({
        onlyConfirmed: statusFilter === "confirmed",
        onlyValidated: statusFilter === "validated",
        projectIds: soloProjectId ? [soloProjectId] : undefined,
      }),
  });
  const projects: DailySummaryProject[] = q.data ?? [];
  const projectList = useMemo(() => projects.map((p) => ({ id: p.projectId, name: p.projectName })), [projects]);

  const agg = useMemo(() => {
    const dayMap = new Map<number, DayCell>();
    const monthTotals = new Map<number, { inc: number; pay: number }>();
    const yearsSet = new Set<number>();

    for (const proj of projects) {
      if (excluded.has(proj.projectId)) continue;
      for (const day of proj.days) {
        const { y, m, d } = ymd(day.date);
        yearsSet.add(y);
        const key = y * 10000 + m * 100 + d;
        let cell = dayMap.get(key);
        if (!cell) {
          cell = { inc: 0, pay: 0, color: TRANSPARENT, priority: -1, incList: [], payList: [] };
          dayMap.set(key, cell);
        }
        cell.inc += day.totalIncomes;
        cell.pay += day.totalPayments;
        // Predominant supplier across the day's projects = the highest VisualPriority with a real color.
        if (day.colorHex !== TRANSPARENT && day.visualPriority > cell.priority) {
          cell.color = day.colorHex;
          cell.priority = day.visualPriority;
        }
        for (const it of day.incomeDetails) {
          cell.incList.push({
            id: it.id, amount: it.amount, percentage: it.percentage, projectId: proj.projectId, projectName: proj.projectName,
            supplierName: null, description: it.description, confirmed: it.confirmed, validated: it.validated, status: it.status,
          });
        }
        for (const it of day.paymentDetails) {
          cell.payList.push({
            id: it.id, amount: it.amount, percentage: it.percentage, projectId: proj.projectId, projectName: proj.projectName,
            supplierName: it.supplierName, description: it.description, confirmed: it.confirmed, validated: it.validated, status: it.status,
          });
        }
        const ym = y * 100 + m;
        const mt = monthTotals.get(ym) ?? { inc: 0, pay: 0 };
        mt.inc += day.totalIncomes;
        mt.pay += day.totalPayments;
        monthTotals.set(ym, mt);
      }
    }

    const keys = [...dayMap.keys()].sort((a, b) => a - b);
    let bal = 0;
    const cum: { ord: number; balance: number }[] = [];
    for (const k of keys) {
      const c = dayMap.get(k)!;
      bal += c.inc - c.pay;
      cum.push({ ord: k, balance: bal });
    }
    return { dayMap, monthTotals, cum, years: [...yearsSet].sort((a, b) => a - b) };
  }, [projects, excluded]);

  const accAsOf = (ord: number): number | null => {
    const cum = agg.cum;
    let lo = 0;
    let hi = cum.length - 1;
    let res: number | null = null;
    while (lo <= hi) {
      const mid = (lo + hi) >> 1;
      if (cum[mid].ord <= ord) {
        res = cum[mid].balance;
        lo = mid + 1;
      } else {
        hi = mid - 1;
      }
    }
    return res;
  };

  const allYears = agg.years.length ? agg.years : [year];
  const minYear = Math.min(...allYears, year);
  const maxYear = Math.max(...allYears, year);

  let yearInc = 0;
  let yearPay = 0;
  for (const [k, c] of agg.dayMap) {
    if (Math.floor(k / 10000) === year) {
      yearInc += c.inc;
      yearPay += c.pay;
    }
  }
  const yearEndBalance = accAsOf(year * 10000 + 12 * 100 + 31);

  const totalProjects = projectList.length;
  const selectedCount = totalProjects - excluded.size;
  const today = { y: now.getFullYear(), m: now.getMonth() + 1, d: now.getDate() };

  // Centrar la vista en el día actual: al cargar y cada vez que se pulsa "Hoy".
  const scrollBoxRef = useRef<HTMLDivElement>(null);
  const todayCellRef = useRef<HTMLTableCellElement>(null);
  const [goToTodayNonce, setGoToTodayNonce] = useState(0);
  const hasData = !!q.data;
  useEffect(() => {
    if (!hasData || year !== today.y) return;
    const box = scrollBoxRef.current;
    const cell = todayCellRef.current;
    if (!box || !cell) return;
    box.scrollTo({
      left: Math.max(0, cell.offsetLeft - box.clientWidth / 2),
      top: Math.max(0, cell.offsetTop - box.clientHeight / 2),
      behavior: goToTodayNonce > 0 ? "smooth" : "auto",
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hasData, year, goToTodayNonce]);

  const headTh =
    "sticky bg-[var(--color-card)] z-20 border border-[var(--color-border)] px-2 py-1.5 text-center text-[11px] font-semibold text-[var(--color-muted-foreground)]";
  const dayTd =
    "sticky left-0 z-10 bg-[var(--color-card)] border border-[var(--color-border)] px-2 py-1 text-center text-[12px] font-medium text-[var(--color-muted-foreground)] tabular-nums";

  return (
    <div className="space-y-5">
      <PageHero
        eyebrow="Proyectos"
        title="Flujo de caja"
        subtitle={
          projects.length ? (
            <span className="flex flex-wrap gap-x-6 gap-y-1 tabular-nums">
              <span>
                Ingresos {year}: <strong className="text-[var(--color-success)]">{money(yearInc)}</strong>
              </span>
              <span>
                Pagos {year}: <strong className="text-[var(--color-destructive)]">{money(yearPay)}</strong>
              </span>
              <span>
                Saldo fin de año:{" "}
                <strong
                  className={
                    yearEndBalance != null && yearEndBalance < 0
                      ? "text-[var(--color-destructive)]"
                      : "text-[var(--color-foreground)]"
                  }
                >
                  {yearEndBalance != null ? money(yearEndBalance) : "—"}
                </strong>
              </span>
            </span>
          ) : (
            "Ingresos y pagos consolidados por día y mes."
          )
        }
      />

      <div className="flex flex-wrap items-center gap-3">
        <div className="flex items-center gap-1 rounded-lg border border-[var(--color-border)] p-0.5">
          <button
            type="button"
            aria-label="Año anterior"
            disabled={year <= minYear}
            onClick={() => setYear((y) => y - 1)}
            className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] disabled:opacity-40"
          >
            <ChevronLeft className="size-4" />
          </button>
          <span className="min-w-[3.5rem] text-center text-[13px] font-semibold tabular-nums text-[var(--color-foreground)]">{year}</span>
          <button
            type="button"
            aria-label="Año siguiente"
            disabled={year >= maxYear}
            onClick={() => setYear((y) => y + 1)}
            className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] disabled:opacity-40"
          >
            <ChevronRight className="size-4" />
          </button>
        </div>

        <Button
          variant="outline"
          onClick={() => {
            setYear(now.getFullYear());
            setGoToTodayNonce((n) => n + 1);
          }}
          className="h-9 gap-1.5 rounded-lg px-3 text-[13px]"
        >
          <CalendarClock className="size-4" />
          Hoy
        </Button>

        <div className="flex items-center gap-0.5 rounded-lg border border-[var(--color-border)] p-0.5">
          {([["all", "Todos"], ["confirmed", "Confirmados"], ["validated", "Validados"]] as const).map(([v, label]) => (
            <button
              key={v}
              type="button"
              onClick={() => setStatusFilter(v)}
              className={
                "rounded-md px-2.5 py-1 text-[12px] font-medium transition-colors " +
                (statusFilter === v
                  ? "bg-[var(--color-primary)] text-[var(--color-primary-foreground)]"
                  : "text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)]")
              }
            >
              {label}
            </button>
          ))}
        </div>

        {soloProjectId ? (
          <span className="inline-flex items-center gap-1.5 rounded-full border border-[var(--color-border)] bg-[var(--color-muted)] py-1 pl-3 pr-1.5 text-[12.5px] text-[var(--color-foreground)]">
            Proyecto: <span className="font-semibold">{projectList[0]?.name ?? "…"}</span>
            <button
              type="button"
              aria-label="Ver todos los proyectos"
              onClick={() => setSearchParams({}, { replace: true })}
              className="grid size-5 cursor-pointer place-items-center rounded-full text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-border)] hover:text-[var(--color-foreground)]"
            >
              <X className="size-3" />
            </button>
          </span>
        ) : (
          <Button variant="outline" onClick={() => setProjectsOpen(true)} className="h-9 gap-1.5 rounded-lg px-3 text-[13px]">
            <ListFilter className="size-4" />
            {selectedCount === totalProjects ? "Todos los proyectos" : `${selectedCount} de ${totalProjects} proyectos`}
          </Button>
        )}
      </div>

      {q.isError ? (
        <div
          role="alert"
          className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]"
        >
          {describe(q.error)}
        </div>
      ) : q.isLoading ? (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-12 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando flujo de caja…
        </div>
      ) : selectedCount === 0 ? (
        <div className="rounded-xl border border-dashed border-[var(--color-border)] px-4 py-12 text-center text-[13px] text-[var(--color-muted-foreground)]">
          No hay proyectos seleccionados. Usa el selector para incluir alguno.
        </div>
      ) : (
        <div
          ref={scrollBoxRef}
          className="overflow-auto rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs"
          style={{ maxHeight: "70vh" }}
        >
          <table className="border-separate" style={{ borderSpacing: 0, minWidth: "max-content" }}>
            <thead>
              <tr>
                <th rowSpan={2} className={dayTd + " top-0 z-30 !font-semibold !text-[var(--color-foreground)]"} style={{ top: 0 }}>
                  Día
                </th>
                {MONTH_NUMS.map((m) => (
                  <th key={m} colSpan={3} className={headTh} style={{ top: 0 }}>
                    {MONTHS[m - 1].toUpperCase()}
                  </th>
                ))}
              </tr>
              <tr>
                {MONTH_NUMS.map((m) => (
                  <Fragment key={m}>
                    <th className={headTh} style={{ top: 30 }}>Pagos</th>
                    <th className={headTh} style={{ top: 30 }}>Ingresos</th>
                    <th className={headTh} style={{ top: 30 }}>Total</th>
                  </Fragment>
                ))}
              </tr>
            </thead>
            <tbody>
              {DAYS.map((d) => (
                <tr key={d}>
                  <td className={dayTd}>{d}</td>
                  {MONTH_NUMS.map((m) => {
                    if (d > daysInMonth(year, m)) {
                      return (
                        <Fragment key={m}>
                          <td className="border border-[var(--color-border)] bg-[oklch(from_var(--color-muted)_l_c_h_/_0.4)]" />
                          <td className="border border-[var(--color-border)] bg-[oklch(from_var(--color-muted)_l_c_h_/_0.4)]" />
                          <td className="border border-[var(--color-border)] bg-[oklch(from_var(--color-muted)_l_c_h_/_0.4)]" />
                        </Fragment>
                      );
                    }
                    const key = year * 10000 + m * 100 + d;
                    const cell = agg.dayMap.get(key);
                    const acc = accAsOf(key);
                    const isToday = today.y === year && today.m === m && today.d === d;
                    const border = isToday ? "2px solid var(--color-success)" : "1px solid var(--color-border)";
                    const weekendBg = isWeekend(year, m, d) ? "oklch(from var(--color-muted) l c h / 0.18)" : undefined;
                    const supplierTint = cell ? hexTint(cell.color, 0.2) : undefined;
                    return (
                      <Fragment key={m}>
                        <td
                          ref={isToday ? todayCellRef : undefined}
                          style={{ border, width: 108, background: supplierTint ?? weekendBg }}
                          title={cell && cell.payList.length && cell.color !== TRANSPARENT ? "Color del proveedor predominante" : undefined}
                          onClick={
                            cell && cell.payList.length
                              ? () => setDetail({ title: "Pagos", date: `${d} de ${MONTHS[m - 1].toLowerCase()} de ${year}`, kind: "pay", entries: cell.payList })
                              : undefined
                          }
                          className={
                            "px-2 py-1 text-right text-[12px] tabular-nums text-[var(--color-destructive)] " +
                            (cell && cell.payList.length ? "cursor-pointer hover:bg-[var(--color-muted)]" : "")
                          }
                        >
                          {cell && cell.pay > 0 ? money(cell.pay) : ""}
                        </td>
                        <td
                          style={{ border, width: 108, background: weekendBg }}
                          onClick={
                            cell && cell.incList.length
                              ? () => setDetail({ title: "Ingresos", date: `${d} de ${MONTHS[m - 1].toLowerCase()} de ${year}`, kind: "inc", entries: cell.incList })
                              : undefined
                          }
                          className={
                            "px-2 py-1 text-right text-[12px] tabular-nums text-[var(--color-success)] " +
                            (cell && cell.incList.length ? "cursor-pointer hover:bg-[var(--color-muted)]" : "")
                          }
                        >
                          {cell && cell.inc > 0 ? money(cell.inc) : ""}
                        </td>
                        <td
                          style={{ border, width: 108 }}
                          className={
                            "px-2 py-1 text-right text-[12px] font-medium tabular-nums bg-[oklch(from_var(--color-muted)_l_c_h_/_0.35)] " +
                            (acc != null && acc < 0 ? "text-[var(--color-destructive)]" : "text-[var(--color-foreground)]")
                          }
                        >
                          {acc != null ? money(acc) : ""}
                        </td>
                      </Fragment>
                    );
                  })}
                </tr>
              ))}
              <tr className="font-semibold">
                <td className={dayTd + " !text-[var(--color-foreground)]"}>Total</td>
                {MONTH_NUMS.map((m) => {
                  const mt = agg.monthTotals.get(year * 100 + m);
                  return (
                    <Fragment key={m}>
                      <td className="border border-[var(--color-border)] px-2 py-1 text-right text-[12px] tabular-nums text-[var(--color-destructive)]">
                        {mt && mt.pay > 0 ? money(mt.pay) : ""}
                      </td>
                      <td className="border border-[var(--color-border)] px-2 py-1 text-right text-[12px] tabular-nums text-[var(--color-success)]">
                        {mt && mt.inc > 0 ? money(mt.inc) : ""}
                      </td>
                      <td className="border border-[var(--color-border)] bg-[oklch(from_var(--color-muted)_l_c_h_/_0.35)]" />
                    </Fragment>
                  );
                })}
              </tr>
            </tbody>
          </table>
        </div>
      )}

      <ProjectsDialog open={projectsOpen} onClose={() => setProjectsOpen(false)} projects={projectList} excluded={excluded} onChange={setExcluded} />
      <DayDetailDialog detail={detail} onClose={() => setDetail(null)} />
    </div>
  );
}

function ProjectsDialog({
  open,
  onClose,
  projects,
  excluded,
  onChange,
}: {
  open: boolean;
  onClose: () => void;
  projects: { id: string; name: string }[];
  excluded: Set<string>;
  onChange: (next: Set<string>) => void;
}) {
  const toggle = (id: string) => {
    const next = new Set(excluded);
    if (next.has(id)) next.delete(id);
    else next.add(id);
    onChange(next);
  };
  return (
    <Dialog open={open} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <DialogHeader>
          <DialogTitle>Proyectos del flujo</DialogTitle>
          <DialogDescription>Incluye o excluye proyectos del cálculo.</DialogDescription>
        </DialogHeader>
        <DialogBody className="space-y-1">
          <div className="mb-2 flex gap-2">
            <Button variant="soft" onClick={() => onChange(new Set())}>Todos</Button>
            <Button variant="outline" onClick={() => onChange(new Set(projects.map((p) => p.id)))}>Ninguno</Button>
          </div>
          <div className="max-h-[50vh] space-y-0.5 overflow-auto">
            {projects.map((p) => {
              const checked = !excluded.has(p.id);
              return (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => toggle(p.id)}
                  className="flex w-full items-center gap-2.5 rounded-md px-2 py-1.5 text-left text-[13px] hover:bg-[var(--color-muted)]"
                >
                  <span
                    className={
                      "grid size-4 place-items-center rounded border " +
                      (checked
                        ? "border-[var(--color-primary)] bg-[var(--color-primary)] text-[var(--color-primary-foreground)]"
                        : "border-[var(--color-border)]")
                    }
                  >
                    {checked && <span className="text-[10px] leading-none">✓</span>}
                  </span>
                  <span className="truncate text-[var(--color-foreground)]">{p.name}</span>
                </button>
              );
            })}
            {projects.length === 0 && <p className="px-2 py-6 text-center text-[13px] text-[var(--color-muted-foreground)]">No hay proyectos.</p>}
          </div>
        </DialogBody>
        <DialogFooter>
          <DialogClose asChild>
            <Button>Listo</Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Tabla de movimientos de un proyecto — Importe / % / Estado / (Proveedor) / Descripción, con el
// ojo de la última columna como acceso al detalle del proyecto.
function DetailTable({ entries, kind, onClose }: { entries: CellDetail[]; kind: "inc" | "pay"; onClose: () => void }) {
  const showSupplier = kind === "pay";
  const th = "px-2.5 py-1.5 text-left text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]";
  const td = "px-2.5 py-1.5 text-[12.5px]";
  return (
    <div className="mt-3 overflow-x-auto rounded-lg border border-[var(--color-border)]">
      <table className="w-full min-w-[560px]">
        <thead>
          <tr className="border-b border-[var(--color-border)]">
            <th className={`${th} text-right`}>Importe</th>
            <th className={`${th} text-right`}>%</th>
            <th className={th}>Estado</th>
            {showSupplier && <th className={th}>Proveedor</th>}
            <th className={th}>Descripción</th>
            <th className={th}>
              <span className="sr-only">Proyecto</span>
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-[var(--color-border)]">
          {entries.map((e) => (
            <tr key={e.id}>
              <td className={`${td} text-right font-medium tabular-nums text-[var(--color-foreground)]`}>
                {money(e.amount)} €
              </td>
              <td className={`${td} text-right tabular-nums text-[var(--color-muted-foreground)]`}>
                {e.percentage != null ? money(e.percentage) : "—"}
              </td>
              <td className={td}>
                <EntityStatusBadge tone={e.validated ? "info" : e.confirmed ? "success" : "default"} withDot>
                  {e.validated ? "Validado" : e.confirmed ? "Confirmado" : "Pendiente"}
                </EntityStatusBadge>
              </td>
              {showSupplier && (
                <td
                  className={`${td} max-w-[160px] truncate text-[var(--color-muted-foreground)]`}
                  title={e.supplierName ?? undefined}
                >
                  {e.supplierName ?? "—"}
                </td>
              )}
              <td
                className={`${td} max-w-[240px] truncate text-[var(--color-muted-foreground)]`}
                title={e.description ?? undefined}
              >
                {e.description || "—"}
              </td>
              <td className={`${td} text-right`}>
                <Link
                  to={`/projects/${e.projectId}`}
                  onClick={onClose}
                  aria-label={`Ver detalles del proyecto ${e.projectName}`}
                  title="Ver detalles del proyecto"
                  className="inline-grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)]"
                >
                  <Eye className="size-4" />
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function DayDetailDialog({ detail, onClose }: { detail: DetailState; onClose: () => void }) {
  const total = detail?.entries.reduce((s, e) => s + e.amount, 0) ?? 0;
  const [query, setQuery] = useState("");

  // Limpiar el buscador cada vez que se abre un día distinto.
  useEffect(() => setQuery(""), [detail]);

  const q = query.trim().toLowerCase();
  const filtered = (detail?.entries ?? []).filter(
    (e) =>
      q === "" ||
      e.projectName.toLowerCase().includes(q) ||
      (e.supplierName ?? "").toLowerCase().includes(q) ||
      (e.description ?? "").toLowerCase().includes(q) ||
      money(e.amount).includes(q),
  );

  // Agrupar por proyecto, como en la app original.
  const groups: { projectId: string; projectName: string; entries: CellDetail[] }[] = [];
  for (const e of filtered) {
    const g = groups.find((x) => x.projectId === e.projectId);
    if (g) g.entries.push(e);
    else groups.push({ projectId: e.projectId, projectName: e.projectName, entries: [e] });
  }

  return (
    <Dialog open={detail != null} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-3xl">
        <DialogHeader>
          <DialogTitle>{detail?.title}</DialogTitle>
          <DialogDescription>
            Día consultado: <strong className="text-[var(--color-foreground)]">{detail?.date}</strong> ·{" "}
            {detail?.entries.length} movimiento(s) · total{" "}
            <strong className={detail?.kind === "inc" ? "text-[var(--color-success)]" : "text-[var(--color-destructive)]"}>
              {money(total)} €
            </strong>
          </DialogDescription>
        </DialogHeader>
        <div className="px-6 pb-2">
          <div className="relative">
            <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-[var(--color-muted-foreground)]" />
            <Input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Buscar por proyecto, proveedor, descripción o importe…"
              className="pl-8"
            />
          </div>
        </div>
        <DialogBody className="max-h-[60vh] space-y-5 overflow-y-auto">
          {groups.map((g) => (
            <section key={g.projectId}>
              <h3 className="border-b-2 border-[var(--color-primary)] pb-1.5 text-[15px] text-[var(--color-foreground)]">
                <span className="font-bold">Proyecto:</span> {g.projectName}
              </h3>
              <DetailTable entries={g.entries} kind={detail?.kind ?? "inc"} onClose={onClose} />
            </section>
          ))}
          {groups.length === 0 && (
            <p className="py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">
              Ningún movimiento coincide con «{query}».
            </p>
          )}
        </DialogBody>
        <DialogFooter>
          <DialogClose asChild>
            <Button variant="outline">Cerrar</Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
