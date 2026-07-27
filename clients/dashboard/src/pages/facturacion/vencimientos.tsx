import { CalendarClock } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  EntityColHeader,
  EntityListCard,
  EntityListHeader,
  EntityPageHeader,
  EntityStatusBadge,
  useTableControls,
} from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { EPS, fmtDia, fmtEur, useFacturacion, type Vencimiento } from "./data";
import { EstadoVencimientoBadge, useRegistrar } from "./shared";

const COLS = "grid grid-cols-[minmax(0,1fr)_84px_minmax(0,1.2fr)_92px_105px_105px_105px_90px_88px] items-center gap-2";

export function VencimientosPage() {
  const { vencimientos, isLoading, isError, error } = useFacturacion();
  const registrar = useRegistrar();

  const ctl = useTableControls<Vencimiento>(vencimientos, {
    referencia: (v) => v.reference,
    tipo: (v) => v.kind,
    contraparte: (v) => v.counterparty,
    vence: (v) => v.dueDate,
    importe: (v) => v.amount,
    pagado: (v) => v.paid,
    pendiente: (v) => v.pending,
    estado: (v) => v.estado,
  });

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={CalendarClock}
        title="Vencimientos"
        total={vencimientos.length || null}
        unit="vencimiento"
        description="Esta es la deuda real: el aging se lee de aquí, no de las facturas."
      />

      {isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(error)}
        </div>
      ) : isLoading ? (
        <p className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando vencimientos…
        </p>
      ) : vencimientos.length === 0 ? (
        <p className="rounded-xl border border-dashed border-[var(--color-border)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Aún no hay vencimientos. Se crean vinculando líneas de caja a una factura (o generando sus hitos).
        </p>
      ) : (
        <EntityListCard>
          <EntityListHeader className={COLS}>
            {(
              [
                ["referencia", "Referencia", undefined],
                ["tipo", "Tipo", undefined],
                ["contraparte", "Contraparte", undefined],
                ["vence", "Vence", undefined],
                ["importe", "Importe", "right"],
                ["pagado", "Pagado", "right"],
                ["pendiente", "Pendiente", "right"],
                ["estado", "Estado", undefined],
              ] as const
            ).map(([key, label, align]) => (
              <EntityColHeader
                key={key}
                colKey={key}
                label={label}
                align={align}
                sort={ctl.sort}
                filters={ctl.filters}
                onSort={ctl.toggleSort}
                onFilter={ctl.setFilter}
              />
            ))}
            <span />
          </EntityListHeader>
          <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
            {ctl.rows.map((v) => (
              <li key={v.id} className={`${COLS} px-5 py-2.5`}>
                <span className="truncate font-mono text-[12px] text-[var(--color-primary)]" title={v.reference}>
                  {v.reference}
                </span>
                <span>
                  <EntityStatusBadge tone={v.kind === "cobro" ? "info" : "default"}>{v.kind}</EntityStatusBadge>
                </span>
                <span className="truncate text-[13px] text-[var(--color-foreground)]" title={v.counterparty}>
                  {v.counterparty}
                </span>
                <span className={`text-[12.5px] tabular-nums ${v.estado === "Vencido" ? "text-[var(--color-destructive)]" : "text-[var(--color-muted-foreground)]"}`}>
                  {fmtDia(v.dueDate)}
                </span>
                <span className="text-right text-[13px] tabular-nums text-[var(--color-foreground)]">{fmtEur(v.amount)}</span>
                <span className="text-right text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{fmtEur(v.paid)}</span>
                <span className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmtEur(v.pending)}</span>
                <span>
                  <EstadoVencimientoBadge estado={v.estado} />
                </span>
                <span className="flex justify-end">
                  {v.pending > EPS && (
                    <Button
                      variant="outline"
                      disabled={registrar.isPending}
                      onClick={() => registrar.mutate({ id: v.id, kind: v.kind })}
                      className="h-7 rounded-md px-2.5 text-[12px]"
                    >
                      Registrar
                    </Button>
                  )}
                </span>
              </li>
            ))}
          </ul>
        </EntityListCard>
      )}
    </div>
  );
}
