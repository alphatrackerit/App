import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { confirmIncome, confirmPayment } from "@/api/projects";
import { EntityStatusBadge } from "@/components/list";
import { describe } from "@/lib/list-helpers";
import type { CobroEstado, VencimientoEstado } from "./data";

export function EstadoVencimientoBadge({ estado }: { estado: VencimientoEstado }) {
  const tone =
    estado === "Pagada" ? "success"
    : estado === "Vencido" ? "danger"
    : estado === "Parcial" ? "warning"
    : estado === "Previsto" ? "info"
    : "default";
  return <EntityStatusBadge tone={tone}>{estado}</EntityStatusBadge>;
}

export function EstadoCobroBadge({ estado }: { estado: CobroEstado }) {
  const tone = estado === "Cobrada" || estado === "Pagada" ? "success" : estado === "Parcial" ? "warning" : "default";
  return <EntityStatusBadge tone={tone}>{estado}</EntityStatusBadge>;
}

/** Registrar el cobro/pago de un vencimiento = confirmar su línea de caja. */
export function useRegistrar() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (vars: { id: string; kind: "cobro" | "pago" }) =>
      vars.kind === "cobro" ? confirmIncome(vars.id) : confirmPayment(vars.id),
    onSuccess: (_d, vars) => {
      toast.success(vars.kind === "cobro" ? "Cobro registrado" : "Pago registrado");
      queryClient.invalidateQueries({ queryKey: ["facturacion"] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      queryClient.invalidateQueries({ queryKey: ["cashflow"] });
      queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
    },
    onError: (err) => toast.error("Error al registrar", { description: describe(err) }),
  });
}

/** Barra de progreso de cobro (pagado / total). */
export function CobroBar({ charged, total }: { charged: number; total: number }) {
  const pct = total > 0 ? Math.min(100, Math.round((charged / total) * 100)) : 0;
  return (
    <span className="block h-1.5 w-20 overflow-hidden rounded-full bg-[var(--color-muted)]" title={`${pct} %`}>
      <span className="block h-full rounded-full bg-[var(--color-success)]" style={{ width: `${pct}%` }} />
    </span>
  );
}
