import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Bird, Drumstick, LayoutDashboard, Wheat } from "lucide-react";
import { getResumenAvicola, type EstadoLote, type LoteResumen } from "@/api/avicola";
import {
  EntityEmpty,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityPageHeader,
} from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

const COLS = "grid-cols-[1fr_120px_80px_100px_90px_110px_90px_80px]";

function fmt(n: number | null | undefined, frac = 0): string {
  if (n === null || n === undefined) return "—";
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: frac }).format(n);
}

const ESTADO_LABEL: Record<EstadoLote, string> = {
  Planificado: "Planificado",
  EnCrianza: "En crianza",
  Finalizado: "Finalizado",
  Cancelado: "Cancelado",
};

function StatCard({
  icon: Icon,
  label,
  value,
  hint,
}: {
  icon: React.ComponentType<{ className?: string }>;
  label: string;
  value: string;
  hint?: string;
}) {
  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4">
      <div className="flex items-center gap-2 text-[12px] font-medium text-[var(--color-muted-foreground)]">
        <Icon className="size-4" />
        {label}
      </div>
      <div className="mt-2 text-[26px] font-semibold tabular-nums text-[var(--color-foreground)]">{value}</div>
      {hint && <div className="mt-0.5 text-[11.5px] text-[var(--color-muted-foreground)]">{hint}</div>}
    </div>
  );
}

export function AvicolaPanelPage() {
  const navigate = useNavigate();
  const query = useQuery({
    queryKey: ["avicola", "resumen"],
    queryFn: getResumenAvicola,
  });

  const data = query.data;
  const lotes: LoteResumen[] = data?.lotes ?? [];

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={LayoutDashboard}
        title="Panel avícola"
        description="Indicadores productivos de la avícola: lotes activos, aves vivas, consumo de alimento y conversión por lote."
      />

      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <StatCard icon={Bird} label="Lotes" value={fmt(data?.totalLotes)} hint={`${fmt(data?.lotesActivos)} en crianza`} />
        <StatCard icon={Bird} label="En crianza" value={fmt(data?.lotesActivos)} />
        <StatCard icon={Drumstick} label="Aves vivas" value={fmt(data?.totalAvesVivas)} />
        <StatCard icon={Wheat} label="Alimento consumido" value={`${fmt(data?.consumoAlimentoTotalKg, 1)} kg`} />
      </div>

      {query.isLoading ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : query.isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(query.error)}
        </div>
      ) : lotes.length === 0 ? (
        <EntityEmpty
          icon={Bird}
          title="Aún no hay lotes"
          body="Crea un lote en la sección Lotes para ver aquí sus indicadores productivos."
        />
      ) : (
        <EntityListCard className="hidden md:block">
          <EntityListHeader className={COLS}>
            <span>Lote</span>
            <span>Estado</span>
            <span className="text-right">Edad</span>
            <span className="text-right">Aves vivas</span>
            <span className="text-right">% Mort.</span>
            <span className="text-right">Alimento</span>
            <span className="text-right">Peso (g)</span>
            <span className="text-right">FCR</span>
          </EntityListHeader>

          {lotes.map((l, i) => (
            <EntityListRow key={l.id} className={COLS} isLast={i === lotes.length - 1} onClick={() => navigate(`/avicola/lotes/${l.id}`)}>
              <div className="min-w-0">
                <div className="truncate text-[14px] font-medium text-[var(--color-foreground)] transition-colors group-hover:text-[var(--color-primary)]">{l.codigo}</div>
                <div className="text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(l.fechaIngreso)}</div>
              </div>
              <div className="text-[12.5px] text-[var(--color-muted-foreground)]">{ESTADO_LABEL[l.estado]}</div>
              <div className="text-right text-[13px] tabular-nums">{fmt(l.edadDias)} d</div>
              <div className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmt(l.avesVivas)}</div>
              <div className="text-right text-[13px] tabular-nums">{fmt(l.porcentajeMortalidad, 1)}%</div>
              <div className="text-right text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{fmt(l.consumoAlimentoKg, 1)} kg</div>
              <div className="text-right text-[13px] tabular-nums">{fmt(l.pesoPromedioGramos)}</div>
              <div className="text-right text-[13px] font-medium tabular-nums">{l.conversionAlimenticia != null ? fmt(l.conversionAlimenticia, 2) : "—"}</div>
            </EntityListRow>
          ))}
        </EntityListCard>
      )}

      {/* Mobile list */}
      {!query.isLoading && !query.isError && lotes.length > 0 && (
        <div className="space-y-2 md:hidden">
          {lotes.map((l) => (
            <button
              key={l.id}
              type="button"
              onClick={() => navigate(`/avicola/lotes/${l.id}`)}
              className="block w-full rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-3 text-left"
            >
              <div className="flex items-center justify-between">
                <span className="text-[14px] font-medium text-[var(--color-foreground)]">{l.codigo}</span>
                <span className="text-[12px] text-[var(--color-muted-foreground)]">{ESTADO_LABEL[l.estado]}</span>
              </div>
              <div className="mt-1 text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">
                {fmt(l.avesVivas)} aves · {fmt(l.porcentajeMortalidad, 1)}% mort. · FCR {l.conversionAlimenticia != null ? fmt(l.conversionAlimenticia, 2) : "—"}
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
