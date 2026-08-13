import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { searchFacturas, type FacturaRow } from "@/api/facturas";
import { searchIncomes, searchPayments, type IncomeDto, type PaymentDto } from "@/api/projects";
import { clientsApi, suppliersApi } from "@/api/administration";
import { generatePaymentMilestones } from "@/lib/payment-terms";

// ───────────────────────────────────────────────────────────────────────
//  Modelo de la sección Facturación, derivado del modelo existente:
//  · Un VENCIMIENTO es una línea de caja (ingreso/pago) vinculada a una factura.
//    "La factura no se fracciona: se fracciona su cobro."
//  · Un vencimiento está PAGADO cuando su línea está confirmada.
//  · El pendiente de una factura = total − líneas confirmadas vinculadas.
// ───────────────────────────────────────────────────────────────────────

export type VencimientoEstado = "Pagada" | "Vencido" | "Parcial" | "Abierto" | "Previsto";

export type Vencimiento = {
  id: string;
  kind: "cobro" | "pago";
  reference: string; // "FRA-2026-003/1"
  invoiceId: string;
  invoiceNumber: string | null;
  counterparty: string;
  supplierId: string | null;
  clientId: string | null;
  companyId: string | null;
  projectId: string | null;
  dueDate: string | null;
  amount: number;
  paid: number;
  pending: number;
  /** % del total de la factura que representa este pago (de la línea o del hito derivado). */
  percentage: number | null;
  /** Nº de pago dentro de su factura (1-based) y total de pagos de esa factura. */
  seq: number;
  seqTotal: number;
  /** true = hito derivado de la forma de pago (sin línea de caja registrada aún). */
  previsto: boolean;
  estado: VencimientoEstado;
};

export type CobroEstado = "Emitida" | "Parcial" | "Cobrada" | "Pagada";
export type InvoiceAgg = FacturaRow & {
  counterparty: string;
  charged: number;
  pendingAmount: number;
  cobroEstado: CobroEstado;
  vencimientos: Vencimiento[];
};

export type Movimiento = {
  id: string;
  kind: "cobro" | "pago";
  amount: number;
  date: string | null;
  counterparty: string;
  description: string | null;
  invoiceId: string | null;
  invoiceNumber: string | null;
  reference: string | null; // vencimiento ref si está vinculado
};

export const EPS = 0.005;

export function fmtEur(n: number): string {
  return `${new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)} €`;
}

export function fmtDia(iso: string | null): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("es-ES");
}

export function useFacturacion() {
  const invoicesQ = useQuery({
    queryKey: ["facturacion", "invoices"],
    queryFn: () => searchFacturas({ pageSize: 10000, sortDir: "desc" }),
    staleTime: 60_000,
  });
  const incomesQ = useQuery({
    queryKey: ["facturacion", "incomes"],
    queryFn: () => searchIncomes({ pageSize: 10000 }),
    staleTime: 60_000,
  });
  const paymentsQ = useQuery({
    queryKey: ["facturacion", "payments"],
    queryFn: () => searchPayments({ pageSize: 10000 }),
    staleTime: 60_000,
  });
  const clientsQ = useQuery({
    queryKey: ["administration", "clients", "options"],
    queryFn: () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    staleTime: 5 * 60_000,
  });
  const suppliersQ = useQuery({
    queryKey: ["administration", "suppliers", "options"],
    queryFn: () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    staleTime: 5 * 60_000,
  });

  const result = useMemo(() => {
    const clientName = new Map((clientsQ.data?.items ?? []).map((c) => [c.id, c.name]));
    const supplierName = new Map((suppliersQ.data?.items ?? []).map((s) => [s.id, s.name]));
    const invoices = invoicesQ.data?.items ?? [];
    const incomes = incomesQ.data?.items ?? [];
    const payments = paymentsQ.data?.items ?? [];
    const byInvoiceInc = new Map<string, IncomeDto[]>();
    const byInvoicePay = new Map<string, PaymentDto[]>();
    for (const i of incomes) {
      if (i.invoiceId) {
        const arr = byInvoiceInc.get(i.invoiceId) ?? [];
        arr.push(i);
        byInvoiceInc.set(i.invoiceId, arr);
      }
    }
    for (const p of payments) {
      if (p.invoiceId) {
        const arr = byInvoicePay.get(p.invoiceId) ?? [];
        arr.push(p);
        byInvoicePay.set(p.invoiceId, arr);
      }
    }

    const todayIso = new Date();
    todayIso.setHours(0, 0, 0, 0);
    const isOverdue = (d: string | null) => !!d && new Date(d) < todayIso;

    const aggregates: InvoiceAgg[] = [];
    const vencimientos: Vencimiento[] = [];
    const lineRef = new Map<string, { reference: string; invoiceNumber: string | null }>();

    for (const f of invoices) {
      const counterparty =
        f.type === "Emitida"
          ? (f.clientId && clientName.get(f.clientId)) || "—"
          : (f.supplierId && supplierName.get(f.supplierId)) || "—";
      const refBase = f.number ?? "(sin número)";
      const lines = (f.type === "Emitida" ? byInvoiceInc.get(f.id) : byInvoicePay.get(f.id)) ?? [];
      const sorted = [...lines].sort((a, b) => (a.date ?? "9999").localeCompare(b.date ?? "9999"));
      let vencs: Vencimiento[] = sorted.map((l, i) => {
        const paid = l.confirmed ? l.amount : 0;
        const pending = l.amount - paid;
        const reference = `${refBase}/${i + 1}`;
        lineRef.set(l.id, { reference, invoiceNumber: f.number });
        return {
          id: l.id,
          kind: f.type === "Emitida" ? ("cobro" as const) : ("pago" as const),
          reference,
          invoiceId: f.id,
          invoiceNumber: f.number,
          counterparty,
          supplierId: "supplierId" in l ? ((l as PaymentDto).supplierId ?? f.supplierId) : null,
          clientId: f.clientId,
          companyId: f.companyId,
          projectId: l.projectId ?? f.projectId,
          dueDate: l.date,
          amount: l.amount,
          paid,
          pending,
          percentage: l.percentage,
          seq: i + 1,
          seqTotal: sorted.length,
          previsto: false,
          estado: pending <= EPS ? ("Pagada" as const) : isOverdue(l.date) ? ("Vencido" as const) : ("Abierto" as const),
        };
      });

      // Sin líneas registradas: proyectar hitos PREVISTOS desde la forma de pago,
      // para ver los pagos futuros (30 % / 70 %…) aunque nadie los haya generado aún.
      if (vencs.length === 0 && f.paymentTerms) {
        const milestones = generatePaymentMilestones(f.paymentTerms, f.total, f.invoiceDate) ?? [];
        vencs = milestones.map((m, i) => ({
          id: `${f.id}-m${i + 1}`,
          kind: f.type === "Emitida" ? ("cobro" as const) : ("pago" as const),
          reference: `${refBase}/${i + 1}`,
          invoiceId: f.id,
          invoiceNumber: f.number,
          counterparty,
          supplierId: f.supplierId,
          clientId: f.clientId,
          companyId: f.companyId,
          projectId: f.projectId,
          dueDate: m.dueDate ?? f.dueDate,
          amount: m.amount,
          paid: 0,
          pending: m.amount,
          percentage: m.percentage,
          seq: i + 1,
          seqTotal: milestones.length,
          previsto: true,
          estado: "Previsto" as const,
        }));
      }
      vencimientos.push(...vencs);

      const charged = vencs.reduce((s, v) => s + v.paid, 0);
      const pendingAmount = f.total - charged;
      const cobroEstado: CobroEstado =
        pendingAmount <= EPS ? (f.type === "Emitida" ? "Cobrada" : "Pagada") : charged > EPS ? "Parcial" : "Emitida";
      aggregates.push({ ...f, counterparty, charged, pendingAmount, cobroEstado, vencimientos: vencs });
    }

    // Movimientos = líneas confirmadas (dinero real), vinculadas o no a factura.
    const invoiceById = new Map(aggregates.map((a) => [a.id, a]));
    const movimientos: Movimiento[] = [];
    for (const l of incomes) {
      if (!l.confirmed) continue;
      const inv = l.invoiceId ? invoiceById.get(l.invoiceId) : undefined;
      movimientos.push({
        id: l.id, kind: "cobro", amount: l.amount, date: l.date, description: l.description,
        counterparty: inv?.counterparty ?? "—", invoiceId: l.invoiceId, invoiceNumber: inv?.number ?? null,
        reference: lineRef.get(l.id)?.reference ?? null,
      });
    }
    for (const l of payments) {
      if (!l.confirmed) continue;
      const inv = l.invoiceId ? invoiceById.get(l.invoiceId) : undefined;
      movimientos.push({
        id: l.id, kind: "pago", amount: l.amount, date: l.date, description: l.description,
        counterparty: inv?.counterparty ?? (l.supplierId && supplierName.get(l.supplierId)) ?? "—",
        invoiceId: l.invoiceId, invoiceNumber: inv?.number ?? null,
        reference: lineRef.get(l.id)?.reference ?? null,
      });
    }
    movimientos.sort((a, b) => (b.date ?? "").localeCompare(a.date ?? ""));
    vencimientos.sort((a, b) => (a.dueDate ?? "9999").localeCompare(b.dueDate ?? "9999"));

    return { invoices: aggregates, vencimientos, movimientos };
  }, [invoicesQ.data, incomesQ.data, paymentsQ.data, clientsQ.data, suppliersQ.data]);

  return {
    ...result,
    isLoading: invoicesQ.isLoading || incomesQ.isLoading || paymentsQ.isLoading,
    isError: invoicesQ.isError || incomesQ.isError || paymentsQ.isError,
    error: invoicesQ.error ?? incomesQ.error ?? paymentsQ.error,
  };
}
