import { Banknote } from "lucide-react";
import {
  EntityColHeader,
  EntityListCard,
  EntityListHeader,
  EntityPageHeader,
  EntityStatusBadge,
  useTableControls,
} from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { fmtDia, fmtEur, useFacturacion, type Movimiento } from "./data";

const COLS = "grid grid-cols-[84px_105px_minmax(0,1.2fr)_92px_minmax(0,1fr)_minmax(0,1fr)_100px] items-center gap-2";

export function PagosCobrosPage() {
  const { movimientos, isLoading, isError, error } = useFacturacion();

  const ctl = useTableControls<Movimiento>(movimientos, {
    tipo: (m) => m.kind,
    importe: (m) => m.amount,
    contraparte: (m) => m.counterparty,
    fecha: (m) => m.date,
    concepto: (m) => m.description,
    referencia: (m) => m.reference ?? (m.invoiceNumber ?? ""),
    asignacion: (m) => (m.invoiceId ? "asignado" : "sin asignar"),
  });

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Banknote}
        title="Pagos y cobros"
        total={movimientos.length || null}
        unit="movimiento"
        description="Dinero real registrado (líneas confirmadas). Un movimiento sin factura asignada no dice qué estás cobrando o pagando."
      />

      {isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(error)}
        </div>
      ) : isLoading ? (
        <p className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando pagos y cobros…
        </p>
      ) : movimientos.length === 0 ? (
        <p className="rounded-xl border border-dashed border-[var(--color-border)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Aún no hay cobros ni pagos registrados.
        </p>
      ) : (
        <EntityListCard>
          <EntityListHeader className={COLS}>
            {(
              [
                ["tipo", "Tipo", undefined],
                ["importe", "Importe", "right"],
                ["contraparte", "Contraparte", undefined],
                ["fecha", "Fecha", undefined],
                ["concepto", "Concepto", undefined],
                ["referencia", "Referencia", undefined],
                ["asignacion", "Asignación", undefined],
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
          </EntityListHeader>
          <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
            {ctl.rows.map((m) => (
              <li key={`${m.kind}-${m.id}`} className={`${COLS} px-5 py-2.5 ${m.invoiceId ? "" : "bg-[oklch(from_var(--color-warning,orange)_l_c_h_/_0.05)]"}`}>
                <span>
                  <EntityStatusBadge tone={m.kind === "cobro" ? "info" : "default"}>{m.kind}</EntityStatusBadge>
                </span>
                <span className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmtEur(m.amount)}</span>
                <span className="truncate text-[13px] text-[var(--color-foreground)]" title={m.counterparty}>
                  {m.counterparty}
                </span>
                <span className="text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">{fmtDia(m.date)}</span>
                <span className="truncate text-[12.5px] text-[var(--color-muted-foreground)]" title={m.description ?? undefined}>
                  {m.description ?? "—"}
                </span>
                <span className="truncate font-mono text-[12px] text-[var(--color-primary)]">{m.reference ?? m.invoiceNumber ?? "—"}</span>
                <span>
                  {m.invoiceId ? (
                    <EntityStatusBadge tone="success">asignado</EntityStatusBadge>
                  ) : (
                    <EntityStatusBadge tone="warning">sin asignar</EntityStatusBadge>
                  )}
                </span>
              </li>
            ))}
          </ul>
        </EntityListCard>
      )}

      <p className="text-[12px] text-[var(--color-muted-foreground)]">
        Los movimientos "sin asignar" pueden vincularse a su factura desde la ficha del proyecto (icono de factura en la línea).
      </p>
    </div>
  );
}
