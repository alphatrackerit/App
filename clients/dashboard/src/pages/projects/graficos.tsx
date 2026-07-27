import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  Legend,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import { getCashflow, searchProjects, type CashflowEntry } from "@/api/projects";
import { PageHero } from "@/components/list";
import { describe } from "@/lib/list-helpers";

const MONTHS = ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

type StatusFilter = "all" | "confirmed" | "validated";

const COLORS = {
  ingresos: "#16a34a",
  pagos: "#dc2626",
  saldo: "#2563eb",
  venta: "#0ea5e9",
  coste: "#f59e0b",
  beneficio: "#16a34a",
  beneficioNeg: "#dc2626",
};

function money(n: number): string {
  return new Intl.NumberFormat("es-ES", { maximumFractionDigits: 0 }).format(n);
}
function compact(n: number): string {
  const a = Math.abs(n);
  if (a >= 1_000_000) return `${(n / 1_000_000).toFixed(1).replace(/\.0$/, "")}M`;
  if (a >= 1_000) return `${Math.round(n / 1_000)}k`;
  return String(Math.round(n));
}
function ymd(iso: string): { y: number; m: number } {
  // Local getters — the backend serializes dates without a timezone offset.
  const dt = new Date(iso);
  return { y: dt.getFullYear(), m: dt.getMonth() + 1 };
}
function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate();
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

export function GraficosPage() {
  const now = new Date();
  const [year, setYear] = useState<number>(now.getFullYear());
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const cashflowQ = useQuery({ queryKey: ["cashflow"], queryFn: getCashflow });
  const projectsQ = useQuery({
    queryKey: ["projects", "list", { all: true }],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });

  const cashflow = cashflowQ.data;

  const { barData, accData, years } = useMemo(() => {
    const pass = (e: CashflowEntry) =>
      statusFilter === "all" || (statusFilter === "confirmed" ? e.confirmed : e.validated);

    // Monthly buckets for the selected year + running balance across all dated entries.
    const monthInc = new Map<number, number>();
    const monthPay = new Map<number, number>();
    const byOrdNet = new Map<number, number>();
    const yearsSet = new Set<number>();

    const collect = (entries: CashflowEntry[], sign: 1 | -1) => {
      for (const e of entries) {
        if (!pass(e)) continue;
        const { y, m } = ymd(e.date);
        yearsSet.add(y);
        if (y === year) {
          const map = sign === 1 ? monthInc : monthPay;
          map.set(m, (map.get(m) ?? 0) + e.amount);
        }
        const d = new Date(e.date).getDate();
        const ord = y * 10000 + m * 100 + d;
        byOrdNet.set(ord, (byOrdNet.get(ord) ?? 0) + sign * e.amount);
      }
    };
    collect(cashflow?.incomes ?? [], 1);
    collect(cashflow?.payments ?? [], -1);

    // Cumulative balance by date.
    const ords = [...byOrdNet.keys()].sort((a, b) => a - b);
    let bal = 0;
    const cum: { ord: number; balance: number }[] = [];
    for (const o of ords) {
      bal += byOrdNet.get(o)!;
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

    const barData = MONTHS.map((name, i) => ({
      name,
      ingresos: monthInc.get(i + 1) ?? 0,
      pagos: monthPay.get(i + 1) ?? 0,
    }));
    const accData = MONTHS.map((name, i) => ({
      name,
      saldo: accAsOf(year * 10000 + (i + 1) * 100 + daysInMonth(year, i + 1)),
    }));

    return { barData, accData, years: [...yearsSet].sort((a, b) => a - b) };
  }, [cashflow, year, statusFilter]);

  const profitData = useMemo(() => {
    const items = projectsQ.data?.items ?? [];
    return items
      .map((p) => ({
        name: p.name,
        venta: p.salePrice ?? 0,
        coste: p.cost ?? 0,
        beneficio: p.profit ?? 0,
      }))
      .sort((a, b) => b.beneficio - a.beneficio);
  }, [projectsQ.data]);

  const allYears = years.length ? years : [year];
  const minYear = Math.min(...allYears, year);
  const maxYear = Math.max(...allYears, year);

  const hasError = cashflowQ.isError || projectsQ.isError;
  const isLoading = cashflowQ.isLoading || projectsQ.isLoading;

  return (
    <div className="space-y-5">
      <PageHero eyebrow="Proyectos" title="Gráficos" subtitle="Ingresos, pagos, saldo y rentabilidad por proyecto." />

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
          {describe(cashflowQ.error ?? projectsQ.error)}
        </div>
      ) : isLoading ? (
        <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-5 py-16 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando gráficos…
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
          <ChartCard title="Ingresos vs Pagos por mes" subtitle={`Año ${year}`}>
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={barData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
                <XAxis dataKey="name" tick={axisTick} tickLine={false} axisLine={{ stroke: "var(--color-border)" }} />
                <YAxis tick={axisTick} tickLine={false} axisLine={false} tickFormatter={compact} width={44} />
                <Tooltip contentStyle={tooltipStyle} cursor={{ fill: "var(--color-muted)", opacity: 0.4 }} formatter={(v) => money(Number(v))} />
                <Legend wrapperStyle={{ fontSize: 12 }} />
                <Bar dataKey="ingresos" name="Ingresos" fill={COLORS.ingresos} radius={[3, 3, 0, 0]} />
                <Bar dataKey="pagos" name="Pagos" fill={COLORS.pagos} radius={[3, 3, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </ChartCard>

          <ChartCard title="Saldo acumulado" subtitle={`Final de cada mes · ${year}`}>
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={accData} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
                <defs>
                  <linearGradient id="saldoFill" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor={COLORS.saldo} stopOpacity={0.35} />
                    <stop offset="100%" stopColor={COLORS.saldo} stopOpacity={0.02} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" vertical={false} />
                <XAxis dataKey="name" tick={axisTick} tickLine={false} axisLine={{ stroke: "var(--color-border)" }} />
                <YAxis tick={axisTick} tickLine={false} axisLine={false} tickFormatter={compact} width={44} />
                <Tooltip contentStyle={tooltipStyle} formatter={(v) => money(Number(v))} />
                <Area type="monotone" dataKey="saldo" name="Saldo" stroke={COLORS.saldo} strokeWidth={2} fill="url(#saldoFill)" />
              </AreaChart>
            </ResponsiveContainer>
          </ChartCard>

          <ChartCard title="Rentabilidad por proyecto" subtitle="Venta · Coste · Beneficio" className="lg:col-span-2">
            {profitData.length === 0 ? (
              <p className="px-2 py-12 text-center text-[13px] text-[var(--color-muted-foreground)]">No hay proyectos.</p>
            ) : (
              <ResponsiveContainer width="100%" height={Math.max(280, profitData.length * 56 + 40)}>
                <BarChart data={profitData} layout="vertical" margin={{ top: 8, right: 16, left: 8, bottom: 0 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" horizontal={false} />
                  <XAxis type="number" tick={axisTick} tickLine={false} axisLine={{ stroke: "var(--color-border)" }} tickFormatter={compact} />
                  <YAxis type="category" dataKey="name" tick={axisTick} tickLine={false} axisLine={false} width={140} />
                  <Tooltip contentStyle={tooltipStyle} cursor={{ fill: "var(--color-muted)", opacity: 0.4 }} formatter={(v) => money(Number(v))} />
                  <Legend wrapperStyle={{ fontSize: 12 }} />
                  <Bar dataKey="venta" name="Venta" fill={COLORS.venta} radius={[0, 3, 3, 0]} />
                  <Bar dataKey="coste" name="Coste" fill={COLORS.coste} radius={[0, 3, 3, 0]} />
                  <Bar dataKey="beneficio" name="Beneficio" radius={[0, 3, 3, 0]}>
                    {profitData.map((d) => (
                      <Cell key={d.name} fill={d.beneficio < 0 ? COLORS.beneficioNeg : COLORS.beneficio} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            )}
          </ChartCard>
        </div>
      )}
    </div>
  );
}

function ChartCard({
  title,
  subtitle,
  className,
  children,
}: {
  title: string;
  subtitle?: string;
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <section className={"rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 shadow-xs " + (className ?? "")}>
      <div className="mb-3 px-1">
        <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">{title}</h2>
        {subtitle && <p className="text-[12px] text-[var(--color-muted-foreground)]">{subtitle}</p>}
      </div>
      {children}
    </section>
  );
}
