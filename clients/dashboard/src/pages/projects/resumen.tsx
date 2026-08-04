import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Area,
  AreaChart,
  CartesianGrid,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  Activity,
  BadgeCheck,
  Building2,
  CalendarClock,
  CalendarRange,
  CircleDashed,
  HandCoins,
  Hourglass,
  TrendingDown,
  Wallet,
} from "lucide-react";
import { getDailySummary, searchProjects, type DailySummaryProject } from "@/api/projects";
import { companiesApi } from "@/api/administration";
import { PageHero } from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { cn } from "@/lib/cn";

const MONTHS = ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

type StatusFilter = "all" | "confirmed" | "validated";
type VentasMode = "ambos" | "contratado" | "real";

const COLORS = { saldo: "#2563eb", proyectado: "#7c3aed" };

function money(n: number): string {
  return `${new Intl.NumberFormat("es-ES", { maximumFractionDigits: 0 }).format(n)} €`;
}
function compact(n: number): string {
  const a = Math.abs(n);
  if (a >= 1_000_000) return `${(n / 1_000_000).toFixed(1).replace(/\.0$/, "")}M`;
  if (a >= 1_000) return `${Math.round(n / 1_000)}k`;
  return String(Math.round(n));
}
function ymd(iso: string): { y: number; m: number; d: number } {
  // Local getters — the backend serializes dates without a timezone offset.
  const dt = new Date(iso);
  return { y: dt.getFullYear(), m: dt.getMonth() + 1, d: dt.getDate() };
}
function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
}
function toOrd(dt: Date): number {
  return dt.getFullYear() * 10000 + (dt.getMonth() + 1) * 100 + dt.getDate();
}
function ordToDate(ord: number): Date {
  return new Date(Math.floor(ord / 10000), Math.floor((ord % 10000) / 100) - 1, ord % 100);
}

const tooltipStyle = {
  background: "var(--color-popover)",
  border: "1px solid var(--color-border)",
  borderRadius: 10,
  fontSize: 12,
  color: "var(--color-popover-foreground)",
  boxShadow: "0 4px 16px oklch(0 0 0 / 0.12)",
} as const;

const axisTick = { fontSize: 11, fill: "var(--color-muted-foreground)" } as const;

export function ResumenPage() {
  const now = new Date();
  const [year, setYear] = useState<number>(now.getFullYear());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [ventasMode, setVentasMode] = useState<VentasMode>("ambos");

  // Full ledger, unfiltered: the "sin confirmar" KPI needs the unconfirmed entries, so the
  // Confirmados/Validados toggle is applied client-side over the per-entry flags instead.
  const summaryQ = useQuery({ queryKey: ["cashflow", "summary", "all"], queryFn: () => getDailySummary({}) });
  const projectsQ = useQuery({
    queryKey: ["projects", "list", { all: true }],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });
  const companiesQ = useQuery({
    queryKey: ["companies", "lookup", { all: true }],
    queryFn: () => companiesApi.search({ pageSize: 10000 }),
  });

  const projects: DailySummaryProject[] = summaryQ.data ?? [];

  const agg = useMemo(() => {
    const pass = (e: { confirmed: boolean; validated: boolean }) =>
      statusFilter === "all" || (statusFilter === "confirmed" ? e.confirmed : e.validated);

    const companyByProject = new Map<string, string | null>();
    for (const p of projectsQ.data?.items ?? []) companyByProject.set(p.id, p.companyId);

    const todayOrd = toOrd(now);
    const in30 = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 30);
    const in90 = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 90);
    const ord30 = toOrd(in30);
    const ord90 = toOrd(in90);
    const thisMonth = now.getFullYear() * 100 + now.getMonth() + 1;
    const prevMonth = now.getMonth() === 0 ? (now.getFullYear() - 1) * 100 + 12 : thisMonth - 1;

    const byOrd = new Map<number, number>(); // net per day (status-filtered)
    const byCompany = new Map<string, number>(); // net as of today, per companyId ("" = sin empresa)
    const yearsSet = new Set<number>();
    let unconfInc30 = 0;
    let unconfPay30 = 0;
    let pay90 = 0;
    let confIncYear = 0;
    let confPayYear = 0;
    let incYear = 0;
    let netThisMonth = 0;
    let netPrevMonth = 0;

    for (const proj of projects) {
      const companyId = companyByProject.get(proj.projectId) ?? "";
      for (const day of proj.days) {
        const { y, m, d } = ymd(day.date);
        const ord = y * 10000 + m * 100 + d;
        const ym = y * 100 + m;
        yearsSet.add(y);
        let inc = 0;
        let pay = 0;
        for (const it of day.incomeDetails) {
          // The unconfirmed KPI is by definition outside the Confirmados filter — always raw.
          if (!it.confirmed && ord >= todayOrd && ord <= ord30) unconfInc30 += it.amount;
          if (!pass(it)) continue;
          inc += it.amount;
          if (it.confirmed && y === year) confIncYear += it.amount;
          if (y === year) incYear += it.amount;
        }
        for (const it of day.paymentDetails) {
          if (!it.confirmed && ord >= todayOrd && ord <= ord30) unconfPay30 += it.amount;
          if (!pass(it)) continue;
          pay += it.amount;
          if (ord >= todayOrd && ord <= ord90) pay90 += it.amount;
          if (it.confirmed && y === year) confPayYear += it.amount;
        }
        const net = inc - pay;
        if (net !== 0) byOrd.set(ord, (byOrd.get(ord) ?? 0) + net);
        if (ym === thisMonth) netThisMonth += net;
        if (ym === prevMonth) netPrevMonth += net;
        if (ord <= todayOrd && net !== 0) byCompany.set(companyId, (byCompany.get(companyId) ?? 0) + net);
      }
    }

    // Cumulative balance across the whole ledger (cross-year), binary-searchable.
    const ords = [...byOrd.keys()].sort((a, b) => a - b);
    let bal = 0;
    const cum: { ord: number; balance: number }[] = [];
    for (const o of ords) {
      bal += byOrd.get(o)!;
      cum.push({ ord: o, balance: bal });
    }
    const accAsOf = (ord: number): number => {
      let lo = 0;
      let hi = cum.length - 1;
      let res = 0;
      while (lo <= hi) {
        const mid = (lo + hi) >> 1;
        if (cum[mid].ord <= ord) {
          res = cum[mid].balance;
          lo = mid + 1;
        } else hi = mid - 1;
      }
      return res;
    };

    const saldoHoy = accAsOf(todayOrd);

    let min90 = saldoHoy;
    for (const c of cum) {
      if (c.ord > todayOrd && c.ord <= ord90 && c.balance < min90) min90 = c.balance;
    }

    // Runway: first projected day the balance dips below zero, over the full future series.
    let runwayDays: number | null = null; // null = never goes negative
    if (saldoHoy < 0) runwayDays = 0;
    else {
      for (const c of cum) {
        if (c.ord > todayOrd && c.balance < 0) {
          runwayDays = Math.round((ordToDate(c.ord).getTime() - ordToDate(todayOrd).getTime()) / 86_400_000);
          break;
        }
      }
    }

    return {
      accAsOf,
      saldoHoy,
      saldoFinAno: accAsOf(year * 10000 + 12 * 100 + 31),
      min90,
      runwayDays,
      unconfInc30,
      unconfPay30,
      pay90,
      netThisMonth,
      netPrevMonth,
      confirmadoNeto: confIncYear - confPayYear,
      cobradoReal: incYear,
      byCompany,
      years: [...yearsSet].sort((a, b) => a - b),
      todayMonth: now.getMonth() + 1,
      todayYear: now.getFullYear(),
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [projects, projectsQ.data, statusFilter, year]);

  const ventasContratadas = useMemo(
    () => (projectsQ.data?.items ?? []).reduce((s, p) => s + (p.salePrice ?? 0), 0),
    [projectsQ.data],
  );

  // Cumulative balance at each month's end; real up to the current month, projected afterwards.
  const chartData = useMemo(
    () =>
      MONTHS.map((name, i) => {
        const m = i + 1;
        const saldo = agg.accAsOf(year * 10000 + m * 100 + daysInMonth(year, m));
        const isPast =
          year < agg.todayYear || (year === agg.todayYear && m <= agg.todayMonth);
        const isFuture =
          year > agg.todayYear || (year === agg.todayYear && m >= agg.todayMonth);
        return {
          name,
          real: isPast ? saldo : null,
          proyectado: isFuture ? saldo : null,
        };
      }),
    [agg, year],
  );

  const companyRows = useMemo(() => {
    const names = new Map((companiesQ.data?.items ?? []).map((c) => [c.id, c.name]));
    return [...agg.byCompany.entries()]
      .map(([id, net]) => ({ id, name: id ? (names.get(id) ?? "…") : "Sin empresa", net }))
      .sort((a, b) => b.net - a.net);
  }, [agg.byCompany, companiesQ.data]);

  const allYears = agg.years.length ? agg.years : [year];
  const minYear = Math.min(...allYears, year);
  const maxYear = Math.max(...allYears, year);

  const hasError = summaryQ.isError || projectsQ.isError || companiesQ.isError;
  const isLoading = summaryQ.isLoading || projectsQ.isLoading;

  const deltaMes = agg.netThisMonth - agg.netPrevMonth;

  return (
    <div className="space-y-5">
      <PageHero eyebrow="Proyectos" title="Resumen" subtitle="Indicadores de caja, proyección y ventas." />

      <div className="flex flex-wrap items-center gap-3">
        <div className="flex items-center gap-1 rounded-lg border border-[var(--color-border)] bg-[var(--color-card)] p-1">
          <button
            type="button"
            aria-label="Año anterior"
            disabled={year <= minYear}
            onClick={() => setYear((y) => y - 1)}
            className="grid size-7 place-items-center rounded-md text-[13px] text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] disabled:opacity-40"
          >
            ‹
          </button>
          <span className="min-w-[56px] text-center font-mono text-[14px] font-semibold tabular-nums text-[var(--color-foreground)]">{year}</span>
          <button
            type="button"
            aria-label="Año siguiente"
            disabled={year >= maxYear}
            onClick={() => setYear((y) => y + 1)}
            className="grid size-7 place-items-center rounded-md text-[13px] text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] disabled:opacity-40"
          >
            ›
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
      </div>

      {hasError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(summaryQ.error ?? projectsQ.error ?? companiesQ.error)}
        </div>
      ) : isLoading ? (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-5 py-16 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando resumen…
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <KpiCard
              index={0}
              label="Saldo de caja (hoy)"
              value={money(agg.saldoHoy)}
              tone={agg.saldoHoy < 0 ? "danger" : "success"}
              icon={Wallet}
            />
            <KpiCard
              index={1}
              label={`Saldo fin de año ${year}`}
              value={money(agg.saldoFinAno)}
              tone={agg.saldoFinAno < 0 ? "danger" : "primary"}
              icon={CalendarRange}
            />
            <KpiCard
              index={2}
              label="Saldo mínimo próx. 90 días"
              value={money(agg.min90)}
              tone={agg.min90 < 0 ? "danger" : "info"}
              icon={TrendingDown}
            />
            <KpiCard
              index={3}
              label="Runway"
              value={agg.runwayDays == null ? "∞" : `${agg.runwayDays} días`}
              sublabel={agg.runwayDays == null ? "Sin déficit proyectado" : "Hasta saldo negativo"}
              tone={agg.runwayDays == null ? "success" : agg.runwayDays <= 90 ? "danger" : "warning"}
              icon={Hourglass}
            />
            <KpiCard
              index={4}
              label="Previsto sin confirmar (30d)"
              value={money(agg.unconfInc30 - agg.unconfPay30)}
              sublabel={`Ingresos ${money(agg.unconfInc30)} · Pagos ${money(agg.unconfPay30)}`}
              tone="warning"
              icon={CircleDashed}
            />
            <KpiCard
              index={5}
              label="Vencimientos próx. 90 días"
              value={money(agg.pay90)}
              sublabel="Pagos con fecha en la ventana"
              tone={agg.pay90 > 0 ? "warning" : "success"}
              icon={CalendarClock}
            />
            <KpiCard
              index={6}
              label="Neto del mes"
              value={money(agg.netThisMonth)}
              sublabel={
                <span className={deltaMes < 0 ? "text-[var(--color-destructive)]" : "text-[var(--color-success)]"}>
                  {deltaMes >= 0 ? "▲" : "▼"} {money(Math.abs(deltaMes))} vs. mes anterior
                </span>
              }
              tone={agg.netThisMonth < 0 ? "danger" : "success"}
              icon={Activity}
            />
            <KpiCard
              index={7}
              label={`Confirmado neto ${year}`}
              value={money(agg.confirmadoNeto)}
              sublabel="Solo movimientos confirmados"
              tone={agg.confirmadoNeto < 0 ? "danger" : "primary"}
              icon={BadgeCheck}
            />
          </div>

          {/* Total ventas — contratado (salePrice) vs. cobrado real (ingresos del año). */}
          <section className="fsh-enter rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-3.5 shadow-xs">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex items-center gap-3">
                <span aria-hidden className="grid size-9 shrink-0 place-items-center rounded-lg bg-[oklch(from_var(--color-primary)_l_c_h_/_0.10)] text-[var(--color-primary)]">
                  <HandCoins className="size-4" />
                </span>
                <p className="text-[11px] font-medium uppercase tracking-wider text-muted-foreground">Total ventas</p>
              </div>
              <div className="flex items-center gap-1 rounded-lg border border-[var(--color-border)] p-0.5">
                {([["ambos", "Ambos"], ["contratado", "Contratado"], ["real", "Real"]] as const).map(([val, label]) => (
                  <button
                    key={val}
                    type="button"
                    onClick={() => setVentasMode(val)}
                    className={
                      "rounded-md px-2.5 py-1 text-[12px] font-medium transition-colors " +
                      (ventasMode === val
                        ? "bg-[var(--color-primary)] text-[var(--color-primary-foreground)]"
                        : "text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)]")
                    }
                  >
                    {label}
                  </button>
                ))}
              </div>
            </div>
            <div className="mt-3 flex flex-wrap gap-x-10 gap-y-2">
              {ventasMode !== "real" && (
                <div>
                  <p className="text-[11px] text-muted-foreground">Ventas contratadas (proyectos)</p>
                  <p className="font-display text-[22px] font-bold leading-tight tabular-nums text-foreground">{money(ventasContratadas)}</p>
                </div>
              )}
              {ventasMode !== "contratado" && (
                <div>
                  <p className="text-[11px] text-muted-foreground">Cobrado real (ingresos {year})</p>
                  <p className="font-display text-[22px] font-bold leading-tight tabular-nums text-foreground">{money(agg.cobradoReal)}</p>
                </div>
              )}
            </div>
          </section>

          <section className="fsh-enter rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 shadow-xs">
            <div className="mb-3 flex items-center gap-2 px-1">
              <Building2 className="size-4 text-[var(--color-muted-foreground)]" />
              <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Por empresa · saldo actual</h2>
            </div>
            {companyRows.length === 0 ? (
              <p className="px-2 py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">Sin movimientos hasta hoy.</p>
            ) : (
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                {companyRows.map((c) => (
                  <div key={c.id || "none"} className="rounded-lg border border-[var(--color-border)] px-3 py-2.5">
                    <p className="truncate text-[12px] font-medium text-[var(--color-muted-foreground)]" title={c.name}>{c.name}</p>
                    <p className={cn("mt-0.5 text-[16px] font-semibold tabular-nums", c.net < 0 ? "text-[var(--color-destructive)]" : "text-[var(--color-foreground)]")}>
                      {money(c.net)}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </section>

          <section className="fsh-enter rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 shadow-xs">
            <div className="mb-3 px-1">
              <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Saldo acumulado</h2>
              <p className="text-[12px] text-[var(--color-muted-foreground)]">Final de cada mes · {year} · real vs. proyectado</p>
            </div>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={chartData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
                <defs>
                  <linearGradient id="resumenRealFill" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor={COLORS.saldo} stopOpacity={0.35} />
                    <stop offset="100%" stopColor={COLORS.saldo} stopOpacity={0.02} />
                  </linearGradient>
                  <linearGradient id="resumenProyFill" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor={COLORS.proyectado} stopOpacity={0.25} />
                    <stop offset="100%" stopColor={COLORS.proyectado} stopOpacity={0.02} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
                <XAxis dataKey="name" tick={axisTick} tickLine={false} axisLine={{ stroke: "var(--color-border)" }} />
                <YAxis tick={axisTick} tickLine={false} axisLine={false} tickFormatter={compact} width={44} />
                <Tooltip contentStyle={tooltipStyle} formatter={(v) => money(Number(v))} />
                <Legend wrapperStyle={{ fontSize: 12 }} />
                <Area type="monotone" dataKey="real" name="Real" stroke={COLORS.saldo} strokeWidth={2} fill="url(#resumenRealFill)" connectNulls={false} />
                <Area
                  type="monotone"
                  dataKey="proyectado"
                  name="Proyectado"
                  stroke={COLORS.proyectado}
                  strokeWidth={2}
                  strokeDasharray="6 4"
                  fill="url(#resumenProyFill)"
                  connectNulls={false}
                />
              </AreaChart>
            </ResponsiveContainer>
          </section>
        </>
      )}
    </div>
  );
}

// ────────────────────────────────────────────────────────────────────────
// KpiCard — local replica of the overview StatCard visual (not exported
// from src/pages/overview.tsx): icon plate + label + big tabular value.
// ────────────────────────────────────────────────────────────────────────

type KpiTone = "primary" | "success" | "warning" | "danger" | "info";

const TONE_BG: Record<KpiTone, string> = {
  primary: "bg-[oklch(from_var(--color-primary)_l_c_h_/_0.10)] text-[var(--color-primary)]",
  success: "bg-[oklch(from_var(--color-success)_l_c_h_/_0.10)] text-[var(--color-success)]",
  warning: "bg-[oklch(from_var(--color-warning)_l_c_h_/_0.12)] text-[var(--color-warning)]",
  danger: "bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.10)] text-[var(--color-destructive)]",
  info: "bg-[oklch(from_var(--color-info)_l_c_h_/_0.10)] text-[var(--color-info)]",
};

function KpiCard({
  index,
  label,
  value,
  sublabel,
  icon: Icon,
  tone,
}: {
  index: number;
  label: string;
  value: React.ReactNode;
  sublabel?: React.ReactNode;
  icon: React.ComponentType<{ className?: string }>;
  tone: KpiTone;
}) {
  return (
    <div
      className="fsh-enter flex h-full items-start gap-3 rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-3.5 shadow-xs transition-colors duration-200 hover:border-[var(--color-border-strong)]"
      style={{ animationDelay: `${index * 50}ms` }}
    >
      <span aria-hidden className={cn("grid size-9 shrink-0 place-items-center rounded-lg", TONE_BG[tone])}>
        <Icon className="size-4" />
      </span>
      <div className="min-w-0 flex-1">
        <p className="text-[11px] font-medium uppercase tracking-wider text-muted-foreground">{label}</p>
        <p className="mt-1 font-display text-[20px] font-bold leading-none tracking-tight tabular-nums text-foreground sm:text-[22px]">
          {value}
        </p>
        {sublabel && <p className="mt-1.5 truncate text-[11px] text-muted-foreground">{sublabel}</p>}
      </div>
    </div>
  );
}
