import { Fragment, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { ChevronLeft, ChevronRight, ListFilter } from "lucide-react";
import { getCashflow, type CashflowEntry } from "@/api/projects";
import { Button } from "@/components/ui/button";
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

type StatusFilter = "all" | "confirmed" | "validated";

function money(n: number): string {
  return new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n);
}
function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
}
function ymd(iso: string): { y: number; m: number; d: number } {
  const dt = new Date(iso);
  return { y: dt.getUTCFullYear(), m: dt.getUTCMonth() + 1, d: dt.getUTCDate() };
}

type DayCell = { inc: number; pay: number; incList: CashflowEntry[]; payList: CashflowEntry[] };

export function FlujoDeCajaPage() {
  const now = new Date();
  const [year, setYear] = useState<number>(now.getUTCFullYear());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [excluded, setExcluded] = useState<Set<string>>(new Set());
  const [projectsOpen, setProjectsOpen] = useState(false);
  const [detail, setDetail] = useState<{ title: string; kind: "inc" | "pay"; entries: CashflowEntry[] } | null>(null);

  const q = useQuery({ queryKey: ["cashflow"], queryFn: getCashflow });
  const data = q.data;

  const projectName = useMemo(() => {
    const m = new Map<string, string>();
    for (const p of data?.projects ?? []) m.set(p.id, p.name);
    return m;
  }, [data]);

  const agg = useMemo(() => {
    const dayMap = new Map<number, DayCell>();
    const monthTotals = new Map<number, { inc: number; pay: number }>();
    const yearsSet = new Set<number>();

    const pass = (e: CashflowEntry) =>
      e.projectId != null &&
      !excluded.has(e.projectId) &&
      (statusFilter === "all" || (statusFilter === "confirmed" ? e.confirmed : e.validated));

    const add = (e: CashflowEntry, kind: "inc" | "pay") => {
      const { y, m, d } = ymd(e.date);
      yearsSet.add(y);
      const key = y * 10000 + m * 100 + d;
      let cell = dayMap.get(key);
      if (!cell) {
        cell = { inc: 0, pay: 0, incList: [], payList: [] };
        dayMap.set(key, cell);
      }
      if (kind === "inc") {
        cell.inc += e.amount;
        cell.incList.push(e);
      } else {
        cell.pay += e.amount;
        cell.payList.push(e);
      }
      const ym = y * 100 + m;
      const mt = monthTotals.get(ym) ?? { inc: 0, pay: 0 };
      if (kind === "inc") mt.inc += e.amount;
      else mt.pay += e.amount;
      monthTotals.set(ym, mt);
    };

    for (const e of data?.incomes ?? []) if (pass(e)) add(e, "inc");
    for (const e of data?.payments ?? []) if (pass(e)) add(e, "pay");

    const keys = [...dayMap.keys()].sort((a, b) => a - b);
    let bal = 0;
    const cum: { ord: number; balance: number }[] = [];
    for (const k of keys) {
      const c = dayMap.get(k)!;
      bal += c.inc - c.pay;
      cum.push({ ord: k, balance: bal });
    }

    return { dayMap, monthTotals, cum, years: [...yearsSet].sort((a, b) => a - b) };
  }, [data, excluded, statusFilter]);

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

  // Year KPI totals
  let yearInc = 0;
  let yearPay = 0;
  for (const [k, c] of agg.dayMap) {
    if (Math.floor(k / 10000) === year) {
      yearInc += c.inc;
      yearPay += c.pay;
    }
  }
  const yearEndBalance = accAsOf(year * 10000 + 12 * 100 + 31);

  const totalProjects = data?.projects.length ?? 0;
  const selectedCount = totalProjects - excluded.size;
  const today = { y: now.getUTCFullYear(), m: now.getUTCMonth() + 1, d: now.getUTCDate() };

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
          data ? (
            <span className="flex flex-wrap gap-x-6 gap-y-1 tabular-nums">
              <span>Ingresos {year}: <strong className="text-[var(--color-success)]">{money(yearInc)}</strong></span>
              <span>Pagos {year}: <strong className="text-[var(--color-destructive)]">{money(yearPay)}</strong></span>
              <span>
                Saldo fin de año:{" "}
                <strong className={yearEndBalance != null && yearEndBalance < 0 ? "text-[var(--color-destructive)]" : "text-[var(--color-foreground)]"}>
                  {yearEndBalance != null ? money(yearEndBalance) : "—"}
                </strong>
              </span>
            </span>
          ) : (
            "Ingresos y pagos consolidados por día y mes."
          )
        }
      />

      {/* Controls */}
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex items-center gap-1 rounded-lg border border-[var(--color-border)] bg-[var(--color-card)] p-1">
          <button
            type="button"
            aria-label="Año anterior"
            disabled={year <= minYear}
            onClick={() => setYear((y) => y - 1)}
            className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] disabled:opacity-40"
          >
            <ChevronLeft className="size-4" />
          </button>
          <span className="min-w-[56px] text-center font-mono text-[14px] font-semibold tabular-nums text-[var(--color-foreground)]">{year}</span>
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

        <div className="flex items-center gap-1 rounded-lg border border-[var(--color-border)] bg-[var(--color-card)] p-1">
          {([["all", "Todos"], ["confirmed", "Confirmados"], ["validated", "Validados"]] as const).map(([val, label]) => (
            <button
              key={val}
              type="button"
              onClick={() => setStatusFilter(val)}
              className={
                "rounded-md px-3 py-1 text-[12.5px] font-medium transition-colors " +
                (statusFilter === val
                  ? "bg-[var(--color-primary)] text-[var(--color-primary-foreground)]"
                  : "text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)]")
              }
            >
              {label}
            </button>
          ))}
        </div>

        <Button variant="outline" onClick={() => setProjectsOpen(true)} className="h-9 gap-2 rounded-lg px-3 text-[12.5px]">
          <ListFilter className="size-4" />
          {selectedCount === totalProjects ? "Todos los proyectos" : `${selectedCount} de ${totalProjects} proyectos`}
        </Button>
      </div>

      {q.isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(q.error)}
        </div>
      ) : q.isLoading ? (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-5 py-16 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando flujo de caja…
        </div>
      ) : selectedCount === 0 ? (
        <div className="rounded-xl border border-dashed border-[var(--color-border)] bg-[var(--color-card)] px-5 py-16 text-center text-[13px] text-[var(--color-muted-foreground)]">
          No hay proyectos seleccionados. Elige al menos uno para ver el flujo.
        </div>
      ) : (
        <div className="overflow-auto rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs" style={{ maxHeight: "70vh" }}>
          <table className="border-separate" style={{ borderSpacing: 0, minWidth: "max-content" }}>
            <thead>
              <tr>
                <th rowSpan={2} className={dayTd + " top-0 z-30 !font-semibold !text-[var(--color-foreground)]"} style={{ top: 0 }}>Día</th>
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
                    return (
                      <Fragment key={m}>
                        <td
                          style={{ border, width: 108 }}
                          onClick={cell && cell.payList.length ? () => setDetail({ title: `Pagos · ${d} ${MONTHS[m - 1]} ${year}`, kind: "pay", entries: cell.payList }) : undefined}
                          className={
                            "px-2 py-1 text-right text-[12px] tabular-nums text-[var(--color-destructive)] " +
                            (cell && cell.payList.length ? "cursor-pointer hover:bg-[var(--color-muted)]" : "")
                          }
                        >
                          {cell && cell.pay > 0 ? money(cell.pay) : ""}
                        </td>
                        <td
                          style={{ border, width: 108 }}
                          onClick={cell && cell.incList.length ? () => setDetail({ title: `Ingresos · ${d} ${MONTHS[m - 1]} ${year}`, kind: "inc", entries: cell.incList }) : undefined}
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

      <ProjectsDialog
        open={projectsOpen}
        onClose={() => setProjectsOpen(false)}
        projects={data?.projects ?? []}
        excluded={excluded}
        onChange={setExcluded}
      />

      <DayDetailDialog detail={detail} projectName={projectName} onClose={() => setDetail(null)} />
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
            <Button variant="soft" onClick={() => onChange(new Set())} className="h-7 rounded-md px-2.5 text-[12px]">Todos</Button>
            <Button variant="outline" onClick={() => onChange(new Set(projects.map((p) => p.id)))} className="h-7 rounded-md px-2.5 text-[12px]">Ninguno</Button>
          </div>
          <div className="max-h-[50vh] space-y-0.5 overflow-auto">
            {projects.map((p) => {
              const checked = !excluded.has(p.id);
              return (
                <button
                  key={p.id}
                  type="button"
                  onClick={() => toggle(p.id)}
                  className="flex w-full items-center gap-2.5 rounded-md px-2 py-1.5 text-left text-[13px] transition-colors hover:bg-[var(--color-muted)]"
                >
                  <span
                    className={
                      "grid size-4 shrink-0 place-items-center rounded border " +
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
            {projects.length === 0 && <p className="px-2 py-4 text-center text-[12.5px] text-[var(--color-muted-foreground)]">No hay proyectos.</p>}
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

function DayDetailDialog({
  detail,
  projectName,
  onClose,
}: {
  detail: { title: string; kind: "inc" | "pay"; entries: CashflowEntry[] } | null;
  projectName: Map<string, string>;
  onClose: () => void;
}) {
  const total = detail?.entries.reduce((s, e) => s + e.amount, 0) ?? 0;
  return (
    <Dialog open={detail != null} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-lg">
        <DialogHeader>
          <DialogTitle>{detail?.title}</DialogTitle>
          <DialogDescription>
            {detail?.entries.length} movimiento(s) · total{" "}
            <strong className={detail?.kind === "inc" ? "text-[var(--color-success)]" : "text-[var(--color-destructive)]"}>{money(total)}</strong>
          </DialogDescription>
        </DialogHeader>
        <DialogBody>
          <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
            {detail?.entries.map((e) => (
              <li key={e.id} className="flex items-center gap-3 py-2">
                <div className="min-w-0 flex-1">
                  <div className="flex items-center gap-2">
                    <span className="text-[14px] font-medium tabular-nums text-[var(--color-foreground)]">{money(e.amount)}</span>
                    {e.confirmed && <EntityStatusBadge tone="success" withDot>Confirmado</EntityStatusBadge>}
                    {e.validated && <EntityStatusBadge tone="info" withDot>Validado</EntityStatusBadge>}
                  </div>
                  <p className="mt-0.5 truncate text-[12px] text-[var(--color-muted-foreground)]">
                    {(e.projectId ? projectName.get(e.projectId) ?? "Proyecto" : "Sin proyecto")}
                    {e.description ? ` · ${e.description}` : ""}
                  </p>
                </div>
              </li>
            ))}
          </ul>
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
