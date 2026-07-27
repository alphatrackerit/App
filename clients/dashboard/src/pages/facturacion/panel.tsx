import { Link } from "react-router-dom";
import { LayoutDashboard } from "lucide-react";
import { Button } from "@/components/ui/button";
import { EntityPageHeader, EntityStatusBadge } from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { EPS, fmtDia, fmtEur, useFacturacion, type Vencimiento } from "./data";
import { EstadoVencimientoBadge, useRegistrar } from "./shared";

function Stat({ label, value, danger }: { label: string; value: string; danger?: boolean }) {
  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-3 shadow-xs">
      <p className="text-[11px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">{label}</p>
      <p className={`mt-1 text-[20px] font-semibold tabular-nums ${danger ? "text-[var(--color-destructive)]" : "text-[var(--color-foreground)]"}`}>
        {value}
      </p>
    </div>
  );
}

function VencRows({ rows, showEstado }: { rows: Vencimiento[]; showEstado?: boolean }) {
  const registrar = useRegistrar();
  const cols = showEstado
    ? "grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)_92px_110px_110px_90px_88px]"
    : "grid-cols-[minmax(0,1fr)_minmax(0,1.2fr)_92px_110px_88px]";
  return (
    <div>
      <div className={`grid ${cols} items-center gap-2 border-b border-[var(--color-border)] px-4 py-2 text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]`}>
        <span>Referencia</span>
        <span>Contraparte</span>
        <span>Vence</span>
        <span className="text-right">{showEstado ? "Importe" : "Pendiente"}</span>
        {showEstado && <span className="text-right">Pendiente</span>}
        {showEstado && <span>Estado</span>}
        <span />
      </div>
      <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
        {rows.map((v) => (
          <li key={v.id} className={`grid ${cols} items-center gap-2 px-4 py-2`}>
            <span className="truncate font-mono text-[12px] text-[var(--color-primary)]">{v.reference}</span>
            <span className="flex min-w-0 items-center gap-1.5 truncate text-[13px] text-[var(--color-foreground)]">
              <span className="truncate">{v.counterparty}</span>
              <EntityStatusBadge tone={v.kind === "cobro" ? "info" : "default"}>{v.kind === "cobro" ? "cliente" : "proveedor"}</EntityStatusBadge>
            </span>
            <span className={`text-[12.5px] tabular-nums ${v.estado === "Vencido" ? "text-[var(--color-destructive)]" : "text-[var(--color-muted-foreground)]"}`}>
              {fmtDia(v.dueDate)}
            </span>
            <span className="text-right text-[13px] tabular-nums text-[var(--color-foreground)]">{fmtEur(showEstado ? v.amount : v.pending)}</span>
            {showEstado && <span className="text-right text-[13px] tabular-nums text-[var(--color-foreground)]">{fmtEur(v.pending)}</span>}
            {showEstado && (
              <span>
                <EstadoVencimientoBadge estado={v.estado} />
              </span>
            )}
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
    </div>
  );
}

export function PanelFacturacionPage() {
  const { invoices, vencimientos, isLoading, isError, error } = useFacturacion();

  const meDeben = invoices.filter((f) => f.type === "Emitida").reduce((s, f) => s + Math.max(0, f.pendingAmount), 0);
  const debo = invoices.filter((f) => f.type === "Recibida").reduce((s, f) => s + Math.max(0, f.pendingAmount), 0);
  const vencidos = vencimientos.filter((v) => v.estado === "Vencido");
  const emitidasCount = invoices.filter((f) => f.type === "Emitida").length;

  const in15 = new Date();
  in15.setDate(in15.getDate() + 15);
  const proximos = vencimientos.filter(
    (v) => v.pending > EPS && v.dueDate && new Date(v.dueDate) <= in15 && v.estado !== "Vencido",
  );

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={LayoutDashboard}
        title="Panel de facturación"
        total={null}
        unit=""
        description="Situación de cobros y pagos. Todo se calcula desde los vencimientos y sus registros, nunca desde un campo editable."
      />

      {isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(error)}
        </div>
      ) : isLoading ? (
        <p className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando facturación…
        </p>
      ) : (
        <>
          <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            <Stat label="Me deben" value={fmtEur(meDeben)} />
            <Stat label="Debo" value={fmtEur(debo)} />
            <Stat label="Vencidos" value={String(vencidos.length)} danger={vencidos.length > 0} />
            <Stat label="Facturas emitidas" value={String(emitidasCount)} />
          </div>

          <section className="overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs">
            <div className="border-b border-[var(--color-border)] px-4 py-3">
              <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Próximos 15 días</h2>
              <p className="text-[12px] text-[var(--color-muted-foreground)]">Ordenado por fecha de vencimiento, no por fecha de factura.</p>
            </div>
            {proximos.length === 0 ? (
              <p className="px-4 py-6 text-center text-[13px] text-[var(--color-muted-foreground)]">Sin vencimientos en los próximos 15 días.</p>
            ) : (
              <VencRows rows={proximos} showEstado />
            )}
          </section>

          <section className="overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs">
            <div className="border-b border-[var(--color-border)] px-4 py-3">
              <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Vencidos</h2>
              <p className="text-[12px] text-[var(--color-muted-foreground)]">Pasada la fecha y con saldo pendiente.</p>
            </div>
            {vencidos.length === 0 ? (
              <p className="px-4 py-6 text-center text-[13px] text-[var(--color-muted-foreground)]">Nada vencido. 🎉</p>
            ) : (
              <VencRows rows={vencidos} />
            )}
          </section>

          <p className="text-[12px] text-[var(--color-muted-foreground)]">
            Los vencimientos son las líneas de caja vinculadas a cada factura — se gestionan en{" "}
            <Link to="/facturacion/facturas" className="text-[var(--color-primary)] hover:underline">
              Facturas
            </Link>{" "}
            (vincular líneas / generar hitos).
          </p>
        </>
      )}
    </div>
  );
}
