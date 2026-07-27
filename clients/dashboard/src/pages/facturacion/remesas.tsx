import { useMemo, useState } from "react";
import { Send } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { EntityListCard, EntityListHeader, EntityPageHeader } from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { EPS, fmtDia, fmtEur, useFacturacion } from "./data";
import { useRegistrar } from "./shared";

const COLS = "grid grid-cols-[32px_minmax(0,1fr)_minmax(0,1.2fr)_100px_120px] items-center gap-2";

export function RemesasPage() {
  const { vencimientos, isLoading, isError, error } = useFacturacion();
  const registrar = useRegistrar();
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [generating, setGenerating] = useState(false);

  // Solo pagos a proveedor pendientes.
  const pendientes = useMemo(() => vencimientos.filter((v) => v.kind === "pago" && v.pending > EPS), [vencimientos]);
  const chosen = pendientes.filter((v) => selected.has(v.id));
  const transferencias = new Set(chosen.map((v) => v.counterparty)).size;
  const total = chosen.reduce((s, v) => s + v.pending, 0);

  const toggle = (id: string) =>
    setSelected((s) => {
      const n = new Set(s);
      if (n.has(id)) n.delete(id);
      else n.add(id);
      return n;
    });
  const toggleAll = () =>
    setSelected((s) => (s.size === pendientes.length ? new Set() : new Set(pendientes.map((v) => v.id))));

  const generar = async () => {
    if (chosen.length === 0) return;
    setGenerating(true);
    try {
      // 1) Fichero de remesa: una transferencia por proveedor (CSV descargable).
      const byProv = new Map<string, { total: number; refs: string[] }>();
      for (const v of chosen) {
        const g = byProv.get(v.counterparty) ?? { total: 0, refs: [] };
        g.total += v.pending;
        g.refs.push(v.reference);
        byProv.set(v.counterparty, g);
      }
      const lines = ["Proveedor;Importe;Referencias"];
      for (const [prov, g] of byProv) lines.push(`${prov};${g.total.toFixed(2)};${g.refs.join(" ")}`);
      const blob = new Blob(["﻿" + lines.join("\r\n")], { type: "text/csv;charset=utf-8" });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `remesa-${new Date().toISOString().slice(0, 10)}.csv`;
      a.click();
      URL.revokeObjectURL(url);

      // 2) Registrar cada pago (confirmar su línea).
      for (const v of chosen) {
        await registrar.mutateAsync({ id: v.id, kind: "pago" });
      }
      toast.success(`Remesa generada: ${transferencias} transferencia(s), ${chosen.length} vencimiento(s).`);
      setSelected(new Set());
    } catch (err) {
      toast.error("La remesa quedó incompleta", { description: describe(err) });
    } finally {
      setGenerating(false);
    }
  };

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Send}
        title="Remesa de pagos a proveedores"
        total={pendientes.length || null}
        unit="vencimiento pendiente"
        description="Selecciona vencimientos; el fichero agrupa una transferencia por proveedor y los pagos quedan registrados."
      />

      {isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(error)}
        </div>
      ) : isLoading ? (
        <p className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando vencimientos…
        </p>
      ) : pendientes.length === 0 ? (
        <p className="rounded-xl border border-dashed border-[var(--color-border)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          No hay pagos a proveedor pendientes.
        </p>
      ) : (
        <>
          <EntityListCard>
            <EntityListHeader className={COLS}>
              <span>
                <input
                  type="checkbox"
                  aria-label="Seleccionar todo"
                  checked={selected.size === pendientes.length && pendientes.length > 0}
                  onChange={toggleAll}
                  className="size-4 cursor-pointer accent-[var(--color-primary)]"
                />
              </span>
              <span>Referencia</span>
              <span>Proveedor</span>
              <span>Vence</span>
              <span className="text-right">Pendiente</span>
            </EntityListHeader>
            <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
              {pendientes.map((v) => (
                <li key={v.id} className={`${COLS} cursor-pointer px-5 py-2.5 hover:bg-[var(--color-muted)]/40`} onClick={() => toggle(v.id)}>
                  <span>
                    <input
                      type="checkbox"
                      aria-label={`Seleccionar ${v.reference}`}
                      checked={selected.has(v.id)}
                      onChange={() => toggle(v.id)}
                      onClick={(e) => e.stopPropagation()}
                      className="size-4 cursor-pointer accent-[var(--color-primary)]"
                    />
                  </span>
                  <span className="truncate font-mono text-[12px] text-[var(--color-primary)]">{v.reference}</span>
                  <span className="truncate text-[13px] text-[var(--color-foreground)]">{v.counterparty}</span>
                  <span className={`text-[12.5px] tabular-nums ${v.estado === "Vencido" ? "text-[var(--color-destructive)]" : "text-[var(--color-muted-foreground)]"}`}>
                    {fmtDia(v.dueDate)}
                  </span>
                  <span className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmtEur(v.pending)}</span>
                </li>
              ))}
            </ul>
          </EntityListCard>

          <div className="grid grid-cols-3 gap-3">
            <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">Vencimientos</p>
              <p className="mt-1 text-[18px] font-semibold tabular-nums">{chosen.length}</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">Transferencias</p>
              <p className="mt-1 text-[18px] font-semibold tabular-nums">{transferencias}</p>
            </div>
            <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-3">
              <p className="text-[11px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">Total</p>
              <p className="mt-1 text-[18px] font-semibold tabular-nums">{fmtEur(total)}</p>
            </div>
          </div>

          <Button disabled={chosen.length === 0 || generating} onClick={generar} className="h-9 rounded-lg px-4 text-[13px]">
            {generating ? "Generando…" : "Generar remesa y pagar"}
          </Button>
        </>
      )}
    </div>
  );
}
