import { useEffect, useState, type FormEvent } from "react";
import { Link, useParams } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, ArrowLeftRight, BadgeCheck, CheckCircle2, FileText, Link2, Pencil, Plus, StickyNote, Trash2, Unlink } from "lucide-react";
import { toast } from "sonner";
import {
  confirmIncome,
  confirmPayment,
  createIncome,
  createNote,
  createPayment,
  deleteIncome,
  deleteNote,
  deletePayment,
  getProject,
  searchIncomes,
  searchNotes,
  searchPayments,
  searchProjects,
  updateIncome,
  updatePayment,
  validateIncome,
  validatePayment,
  type PagedResponse,
} from "@/api/projects";
import { ProjectEditorDialog } from "@/pages/projects/projects";
import {
  clientsApi,
  companiesApi,
  countriesApi,
  prefixesApi,
  societiesApi,
  statusesApi,
} from "@/api/administration";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogBody,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Combobox, type ComboboxOption, EntityStatusBadge, Field } from "@/components/list";
import { suppliersApi } from "@/api/administration";
import { getFactura, linkLine, searchFacturas } from "@/api/facturas";
import { describe, formatDate } from "@/lib/list-helpers";

function money(n: number | null | undefined): string {
  if (n === null || n === undefined) return "—";
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(n);
}

function toNum(s: string): number | null {
  const t = s.trim();
  if (t === "") return null;
  const n = Number(t);
  return Number.isFinite(n) ? n : null;
}

function dateToIso(s: string): string | null {
  const t = s.trim();
  if (t === "") return null;
  const d = new Date(t);
  return Number.isNaN(d.getTime()) ? null : d.toISOString();
}

// Resolve a catalog id → name (shares the "options" cache with the editors).
function useLookupName(
  key: string,
  api: { search: (params?: { pageSize?: number; sortBy?: string; sortDir?: "asc" | "desc" }) => Promise<{ items: { id: string; name: string }[] }> },
  id: string | null | undefined,
): string | null {
  const q = useQuery({
    queryKey: ["administration", key, "options"],
    queryFn: () => api.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: !!id,
  });
  if (!id) return null;
  return q.data?.items.find((i) => i.id === id)?.name ?? null;
}

function HeaderField({ label, value }: { label: string; value: string | null }) {
  return (
    <div className="flex items-baseline gap-1.5 text-[13px]">
      <span className="shrink-0 font-semibold text-[var(--color-foreground)]">{label}:</span>
      <span className="truncate text-[var(--color-muted-foreground)]">{value ?? "—"}</span>
    </div>
  );
}

export function ProjectDetailPage() {
  const { projectId = "" } = useParams();
  const [editing, setEditing] = useState(false);

  const query = useQuery({
    queryKey: ["projects", "detail", projectId],
    queryFn: () => getProject(projectId),
    enabled: projectId.length > 0,
  });

  const p = query.data;
  const clientName = useLookupName("clients", clientsApi, p?.clientId);
  const societyName = useLookupName("societies", societiesApi, p?.societyId);
  const countryName = useLookupName("countries", countriesApi, p?.countryId);
  const companyName = useLookupName("companies", companiesApi, p?.companyId);
  const statusName = useLookupName("statuses", statusesApi, p?.statusId);
  const categoryName = useLookupName("prefixes", prefixesApi, p?.prefixId);

  const pct =
    p?.profit != null && p?.salePrice ? `${new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format((p.profit / p.salePrice) * 100)} %` : null;
  const eur = (n: number | null | undefined) => (n === null || n === undefined ? null : `${money(n)} €`);

  return (
    <div className="space-y-6">
      {/* Legacy-style header card: title + metadata grid + action bar */}
      <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-5 py-4 shadow-xs">
        <h1 className="text-center font-display text-[20px] text-[var(--color-muted-foreground)]">
          Proyecto{" "}
          <span className="font-semibold text-[var(--color-foreground)] underline decoration-[var(--color-primary)] decoration-2 underline-offset-8">
            {p?.name ?? (query.isLoading ? "Cargando…" : "—")}
          </span>
        </h1>

        <div className="mt-5 grid grid-cols-1 gap-x-8 gap-y-2.5 sm:grid-cols-2 lg:grid-cols-3">
          <HeaderField label="Cliente" value={clientName} />
          <HeaderField label="Sociedad" value={societyName} />
          <HeaderField label="País" value={countryName} />
          <HeaderField label="Estado" value={statusName} />
          <HeaderField label="Categoría" value={categoryName} />
          <HeaderField label="Empresa" value={companyName} />
          <HeaderField label="Venta" value={eur(p?.salePrice)} />
          <HeaderField label="Venta prevista" value={eur(p?.forecastSale)} />
          <HeaderField label="Coste" value={eur(p?.cost)} />
          <HeaderField label="Coste previsto" value={eur(p?.forecastCost)} />
          <HeaderField label="Beneficio" value={eur(p?.profit)} />
          <HeaderField label="%" value={pct} />
        </div>

        <div className="mt-4 flex flex-wrap gap-2 border-t border-[var(--color-border)] pt-4">
          <Button asChild variant="outline" className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
            <Link to={`/flujo-de-caja?proyecto=${encodeURIComponent(projectId)}`}>
              <ArrowLeftRight className="size-3.5" />
              Ver reporte
            </Link>
          </Button>
          <Button variant="outline" onClick={() => setEditing(true)} disabled={!p} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
            <Pencil className="size-3.5" />
            Editar
          </Button>
          <Button asChild variant="outline" className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
            <Link to="/projects">
              <ArrowLeft className="size-3.5" />
              Volver
            </Link>
          </Button>
          <Button asChild variant="outline" className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
            <a href="#notas">
              <StickyNote className="size-3.5" />
              Notas
            </a>
          </Button>
        </div>
      </section>

      {query.isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(query.error)}
        </div>
      ) : (
        <>
          {/* Ingresos y Pagos lado a lado, como en la app original */}
          <div className="grid grid-cols-1 items-start gap-6 xl:grid-cols-2">
            <LedgerSection kind="income" projectId={projectId} title="Ingresos" />
            <LedgerSection kind="payment" projectId={projectId} title="Pagos" />
          </div>
          <NotesSection projectId={projectId} />
        </>
      )}

      {p && <ProjectEditorDialog state={editing ? { mode: "edit", project: p } : { mode: "closed" }} onClose={() => setEditing(false)} />}
    </div>
  );
}

// ───────────────────────────────────────────────────────────────────────
//  Ledger (Incomes / Payments) — same shape, different endpoints.
// ───────────────────────────────────────────────────────────────────────

type LedgerItem = {
  id: string;
  amount: number;
  description: string | null;
  date: string | null;
  percentage: number | null;
  projectId: string | null;
  statusId: string | null;
  supplierId?: string | null;
  confirmed: boolean;
  validated: boolean;
  invoiceId: string | null;
};

type LedgerValues = {
  amount: number;
  description: string | null;
  date: string | null;
  percentage: number | null;
  supplierId: string | null;
  // Solo en edición (en creación el registro nace en el proyecto actual):
  projectId: string | null;
  confirmed: boolean;
  validated: boolean;
};

const LEDGER = {
  income: {
    queryKey: "incomes",
    search: searchIncomes,
    create: (projectId: string, v: LedgerValues) =>
      createIncome({ projectId, amount: v.amount, description: v.description, date: v.date, percentage: v.percentage }),
    // El estado se deriva de los booleanos; statusId se reenvía tal cual para no perder el dato legado.
    update: (item: LedgerItem, v: LedgerValues) =>
      updateIncome(item.id, {
        amount: v.amount,
        description: v.description,
        date: v.date,
        percentage: v.percentage,
        projectId: v.projectId,
        statusId: item.statusId,
      }),
    confirm: confirmIncome,
    validate: validateIncome,
    remove: deleteIncome,
  },
  payment: {
    queryKey: "payments",
    search: searchPayments,
    create: (projectId: string, v: LedgerValues) =>
      createPayment({ projectId, amount: v.amount, description: v.description, date: v.date, percentage: v.percentage, supplierId: v.supplierId }),
    update: (item: LedgerItem, v: LedgerValues) =>
      updatePayment(item.id, {
        amount: v.amount,
        description: v.description,
        date: v.date,
        percentage: v.percentage,
        supplierId: v.supplierId,
        projectId: v.projectId,
        statusId: item.statusId,
      }),
    confirm: confirmPayment,
    validate: validatePayment,
    remove: deletePayment,
  },
} as const;

const LEDGER_COLS = "grid grid-cols-[28px_32px_92px_minmax(0,1fr)_84px_64px_84px] items-center gap-x-2";

function LedgerSection({ kind, projectId, title }: { kind: "income" | "payment"; projectId: string; title: string }) {
  const cfg = LEDGER[kind];
  const queryClient = useQueryClient();
  const [editor, setEditor] = useState<{ open: boolean; item: LedgerItem | null }>({ open: false, item: null });
  const [linking, setLinking] = useState<LedgerItem | null>(null);
  const key = ["projects", "detail", projectId, cfg.queryKey];

  const query = useQuery({
    queryKey: key,
    queryFn: () => cfg.search({ projectId, pageSize: 100, sortBy: "date", sortDir: "desc" }) as Promise<PagedResponse<LedgerItem>>,
    enabled: projectId.length > 0,
    placeholderData: keepPreviousData,
  });

  const suppliersQ = useQuery({
    queryKey: ["administration", "suppliers", "options"],
    queryFn: () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: kind === "payment",
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: key });
    // Also refresh the Flujo de caja grid + Gráficos, which read the ["cashflow"] ledger,
    // and the Facturas views (line links change reconciliation totals).
    queryClient.invalidateQueries({ queryKey: ["cashflow"] });
    queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
  };

  const confirm = useMutation({
    mutationFn: (vars: { id: string; value: boolean }) => cfg.confirm(vars.id, vars.value),
    onSuccess: (_d, vars) => { toast.success(vars.value ? "Confirmado" : "Confirmación quitada"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const validate = useMutation({
    mutationFn: (vars: { id: string; value: boolean }) => cfg.validate(vars.id, vars.value),
    onSuccess: (_d, vars) => { toast.success(vars.value ? "Validado" : "Validación quitada"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const remove = useMutation({
    mutationFn: (id: string) => cfg.remove(id),
    onSuccess: () => { toast.success("Borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const items = query.data?.items ?? [];
  const total = items.reduce((sum, i) => sum + i.amount, 0);

  return (
    <section className="overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs">
      <div className="flex items-center justify-between border-b border-[var(--color-border)] px-5 py-3.5">
        <div className="flex items-baseline gap-2">
          <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">{title}</h2>
          <span className="font-mono text-[11px] text-[var(--color-muted-foreground)] tabular-nums">
            {items.length} · total {money(total)}
          </span>
        </div>
        <Button variant="soft" onClick={() => setEditor({ open: true, item: null })} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
          <Plus className="size-3.5" />
          Crear
        </Button>
      </div>

      {items.length === 0 ? (
        <p className="px-5 py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">
          {query.isLoading ? "Cargando…" : `Sin ${title.toLowerCase()} todavía.`}
        </p>
      ) : (
        <div>
          {/* Cabecera de la tabla — C (confirmado) | Validar | Importe | Desc | Fecha | % | acciones */}
          <div className={`${LEDGER_COLS} border-b border-[var(--color-border)] bg-[var(--color-muted)]/40 px-4 py-2 text-[11px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]`}>
            <span title="Confirmado">C</span>
            <span>Val.</span>
            <span className="text-right">Importe</span>
            <span>Desc</span>
            <span>Fecha</span>
            <span className="text-right">%</span>
            <span />
          </div>
          <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.4)]">
            {items.map((it) => (
              <li
                key={it.id}
                onClick={() => setEditor({ open: true, item: it })}
                title="Editar registro"
                className={`group ${LEDGER_COLS} cursor-pointer px-4 py-2.5 transition-colors ${
                  it.confirmed && it.validated
                    ? "bg-[oklch(from_var(--color-success)_l_c_h_/_0.08)] hover:bg-[oklch(from_var(--color-success)_l_c_h_/_0.14)]"
                    : "hover:bg-[var(--color-muted)]/40"
                }`}
              >
                <button
                  type="button"
                  title={it.confirmed ? "Quitar confirmación" : "Confirmar"}
                  aria-label={it.confirmed ? "Quitar confirmación" : "Confirmar"}
                  disabled={confirm.isPending}
                  onClick={(e) => {
                    e.stopPropagation();
                    confirm.mutate({ id: it.id, value: !it.confirmed });
                  }}
                  className={`grid size-6 cursor-pointer place-items-center rounded-md ${
                    it.confirmed
                      ? "text-[var(--color-success)] hover:opacity-70"
                      : "text-[oklch(from_var(--color-muted-foreground)_l_c_h_/_0.4)] hover:text-[var(--color-success)]"
                  }`}
                >
                  <CheckCircle2 className="size-4" />
                </button>
                <button
                  type="button"
                  title={it.validated ? "Quitar validación" : "Validar"}
                  aria-label={it.validated ? "Quitar validación" : "Validar"}
                  disabled={validate.isPending}
                  onClick={(e) => {
                    e.stopPropagation();
                    validate.mutate({ id: it.id, value: !it.validated });
                  }}
                  className={`grid size-6 cursor-pointer place-items-center rounded-md ${
                    it.validated
                      ? "text-[var(--color-info)] hover:opacity-70"
                      : "text-[oklch(from_var(--color-muted-foreground)_l_c_h_/_0.4)] hover:text-[var(--color-info)]"
                  }`}
                >
                  <BadgeCheck className="size-4" />
                </button>
                <span className="self-center text-right text-[13px] font-medium text-[var(--color-foreground)] tabular-nums">
                  {money(it.amount)} €
                </span>
                <span className="min-w-0 self-center truncate text-[13px] text-[var(--color-foreground)]" title={it.description ?? undefined}>
                  {it.description ?? "—"}
                </span>
                <span className="self-center text-[12.5px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(it.date)}</span>
                <span className="self-center text-right text-[12.5px] text-[var(--color-muted-foreground)] tabular-nums">
                  {it.percentage != null ? `${money(it.percentage)} %` : "—"}
                </span>
                <span className="flex items-center justify-end gap-0.5">
                  <button
                    type="button"
                    title={it.invoiceId ? "Con factura vinculada — clic para gestionar" : "Sin factura — clic para vincular"}
                    aria-label={it.invoiceId ? "Con factura vinculada — clic para gestionar" : "Sin factura — clic para vincular"}
                    onClick={(e) => {
                      e.stopPropagation();
                      setLinking(it);
                    }}
                    className="grid size-6 cursor-pointer place-items-center rounded-md transition-colors hover:bg-[var(--color-muted)]"
                  >
                    <FileText
                      aria-hidden
                      className={`size-3.5 ${
                        it.invoiceId ? "text-[var(--color-primary)]" : "text-[oklch(from_var(--color-muted-foreground)_l_c_h_/_0.35)]"
                      }`}
                    />
                  </button>
                  <button
                    type="button"
                    title="Editar"
                    aria-label="Editar"
                    onClick={(e) => {
                      e.stopPropagation();
                      setEditor({ open: true, item: it });
                    }}
                    className="grid size-6 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)]"
                  >
                    <Pencil className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    title="Borrar"
                    aria-label="Borrar"
                    disabled={remove.isPending}
                    onClick={(e) => {
                      e.stopPropagation();
                      remove.mutate(it.id);
                    }}
                    className="grid size-6 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
                  >
                    <Trash2 className="size-3.5" />
                  </button>
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}

      <AmountDialog
        open={editor.open}
        item={editor.item}
        kind={kind}
        title={title}
        supplierOptions={
          kind === "payment" ? (suppliersQ.data?.items ?? []).map((s) => ({ value: s.id, label: s.name })) : undefined
        }
        onClose={() => setEditor({ open: false, item: null })}
        onSubmit={async (values, item) => {
          if (item) {
            await cfg.update(item, values);
            // Los flags viajan por sus endpoints propios, en ambos sentidos.
            if (values.confirmed !== item.confirmed) await cfg.confirm(item.id, values.confirmed);
            if (values.validated !== item.validated) await cfg.validate(item.id, values.validated);
          } else {
            await cfg.create(projectId, values);
          }
          toast.success(item ? "Actualizado" : "Creado");
          invalidate();
          setEditor({ open: false, item: null });
        }}
      />

      <LinkInvoiceDialog kind={kind} item={linking} onClose={() => setLinking(null)} onDone={invalidate} />
    </section>
  );
}

// ── Vincular / desvincular una línea de caja a su factura, desde la propia fila ──

function LinkInvoiceDialog({
  kind,
  item,
  onClose,
  onDone,
}: {
  kind: "income" | "payment";
  item: LedgerItem | null;
  onClose: () => void;
  onDone: () => void;
}) {
  const isOpen = item !== null;
  const side = kind === "income" ? ("Emitida" as const) : ("Recibida" as const);
  const lineKind = kind === "income" ? ("Income" as const) : ("Payment" as const);

  const [search, setSearch] = useState("");
  const [debounced, setDebounced] = useState("");
  useEffect(() => {
    const t = setTimeout(() => setDebounced(search.trim()), 250);
    return () => clearTimeout(t);
  }, [search]);
  useEffect(() => {
    if (isOpen) {
      setSearch("");
      setDebounced("");
    }
  }, [isOpen]);

  // La factura actualmente vinculada (para mostrar número/total antes de desvincular).
  const currentQ = useQuery({
    queryKey: ["administration", "facturas", "byId", item?.invoiceId],
    queryFn: () => getFactura(item!.invoiceId!),
    enabled: isOpen && !!item?.invoiceId,
  });

  const candidatesQ = useQuery({
    queryKey: ["administration", "facturas", "pick", side, debounced],
    queryFn: () => searchFacturas({ type: side, search: debounced || undefined, pageSize: 8, sortDir: "desc" }),
    enabled: isOpen,
    placeholderData: keepPreviousData,
  });

  const mutate = useMutation({
    // Per-call data travels through mutate(vars), never closed-over state.
    mutationFn: (vars: { lineId: string; invoiceId: string | null }) =>
      linkLine({ lineId: vars.lineId, kind: lineKind, invoiceId: vars.invoiceId }),
    onSuccess: (_d, vars) => {
      toast.success(vars.invoiceId ? "Vinculado a factura" : "Desvinculado de la factura");
      onDone();
      onClose();
    },
    onError: (err) => toast.error("Error al vincular", { description: describe(err) }),
  });

  const candidates = (candidatesQ.data?.items ?? []).filter((f) => f.id !== item?.invoiceId);

  // Nombres para la ficha de la línea y las columnas de las candidatas.
  const suppliersQ = useQuery({
    queryKey: ["administration", "suppliers", "options"],
    queryFn: () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen && kind === "payment",
  });
  const clientsQ = useQuery({
    queryKey: ["administration", "clients", "options"],
    queryFn: () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen && kind === "income",
  });
  const companiesQ = useQuery({
    queryKey: ["administration", "companies", "options"],
    queryFn: () => companiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen,
  });
  const nameOf = (items: { id: string; name: string }[] | undefined, id: string | null | undefined): string =>
    (id && items?.find((i) => i.id === id)?.name) || "—";
  const counterpartyOf = (f: { clientId: string | null; supplierId: string | null }): string =>
    kind === "income" ? nameOf(clientsQ.data?.items, f.clientId) : nameOf(suppliersQ.data?.items, f.supplierId);

  const infoRow = "flex items-baseline gap-1.5 text-[12.5px]";
  const infoLabel = "shrink-0 font-semibold text-[var(--color-foreground)]";
  const infoValue = "truncate text-[var(--color-muted-foreground)]";

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md sm:!max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Link2 className="size-4 text-[var(--color-primary)]" />
            {kind === "income" ? "Factura del ingreso" : "Factura del pago"}
          </DialogTitle>
        </DialogHeader>
        <DialogBody className="space-y-4">
          {item && (
            <div className="rounded-lg border border-[var(--color-border)] bg-[var(--color-muted)]/30 px-3 py-2.5">
              <div className="grid grid-cols-2 gap-x-6 gap-y-1.5 sm:grid-cols-3">
                <div className={infoRow}>
                  <span className={infoLabel}>Importe:</span>
                  <span className="truncate font-medium text-[var(--color-foreground)] tabular-nums">{money(item.amount)} €</span>
                </div>
                <div className={infoRow}>
                  <span className={infoLabel}>Fecha:</span>
                  <span className={infoValue}>{item.date ? formatDate(item.date) : "—"}</span>
                </div>
                <div className={infoRow}>
                  <span className={infoLabel}>%:</span>
                  <span className={infoValue}>{item.percentage != null ? money(item.percentage) : "—"}</span>
                </div>
                {kind === "payment" && (
                  <div className={infoRow}>
                    <span className={infoLabel}>Proveedor:</span>
                    <span className={infoValue}>{nameOf(suppliersQ.data?.items, item.supplierId)}</span>
                  </div>
                )}
                <div className={infoRow}>
                  <span className={infoLabel}>Estado:</span>
                  <EntityStatusBadge tone={item.validated ? "info" : item.confirmed ? "success" : "default"} withDot>
                    {item.validated ? "Validado" : item.confirmed ? "Confirmado" : "Pendiente"}
                  </EntityStatusBadge>
                </div>
                <div className={`${infoRow} col-span-2 sm:col-span-3`}>
                  <span className={infoLabel}>Descripción:</span>
                  <span className={infoValue} title={item.description ?? undefined}>{item.description ?? "—"}</span>
                </div>
              </div>
            </div>
          )}

          {item?.invoiceId && (
            <div className="flex items-center gap-2 rounded-lg border border-[var(--color-border)] px-3 py-2">
              <FileText className="size-4 shrink-0 text-[var(--color-primary)]" />
              <span className="min-w-0 flex-1 truncate text-[13px] text-[var(--color-foreground)]">
                {currentQ.data
                  ? `${currentQ.data.number} · ${money(currentQ.data.total)}`
                  : currentQ.isLoading
                    ? "Cargando factura…"
                    : "Factura vinculada"}
              </span>
              <Button
                type="button"
                variant="outline"
                disabled={mutate.isPending}
                onClick={() => mutate.mutate({ lineId: item.id, invoiceId: null })}
                className="h-7 shrink-0 rounded-md px-2.5 text-[12px]"
              >
                <Unlink className="mr-1 size-3" />
                Desvincular
              </Button>
            </div>
          )}

          <div>
            <p className="mb-1.5 text-[12px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
              {item?.invoiceId ? "Cambiar a otra factura" : `Vincular a factura ${side.toLowerCase()}`}
            </p>
            <Input
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Buscar por número…"
              aria-label="Buscar factura por número"
            />
            <div className="mt-2">
              {candidatesQ.isLoading ? (
                <p className="text-[13px] text-[var(--color-muted-foreground)]">Cargando…</p>
              ) : candidates.length === 0 ? (
                <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-2 text-[13px] text-[var(--color-muted-foreground)]">
                  {debounced ? "Sin facturas que coincidan." : `No hay facturas ${side.toLowerCase()}s.`}
                </p>
              ) : (
                <div className="overflow-hidden rounded-lg border border-[var(--color-border)]">
                  <div className="grid grid-cols-[minmax(0,1.1fr)_minmax(0,1fr)_78px_78px_90px_84px] items-center gap-2 border-b border-[var(--color-border)] bg-[var(--color-muted)]/40 px-3 py-1.5 text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
                    <span>Número</span>
                    <span>{kind === "income" ? "Cliente" : "Proveedor"}</span>
                    <span>Fecha</span>
                    <span>Vencim.</span>
                    <span className="text-right">Total</span>
                    <span />
                  </div>
                  <ul className="divide-y divide-[var(--color-border)]">
                    {candidates.map((f) => (
                      <li key={f.id} className="grid grid-cols-[minmax(0,1.1fr)_minmax(0,1fr)_78px_78px_90px_84px] items-center gap-2 px-3 py-2">
                        <span className="min-w-0 truncate text-[13px] font-medium text-[var(--color-foreground)]" title={f.number ?? "(sin número)"}>
                          {f.number ?? "(sin número)"}
                        </span>
                        <span className="min-w-0 truncate text-[12.5px] text-[var(--color-muted-foreground)]" title={counterpartyOf(f)}>
                          {counterpartyOf(f)}
                          {f.companyId ? (
                            <span className="block truncate text-[11px] text-[oklch(from_var(--color-muted-foreground)_l_c_h_/_0.7)]">
                              {nameOf(companiesQ.data?.items, f.companyId)}
                            </span>
                          ) : null}
                        </span>
                        <span className="text-[12px] tabular-nums text-[var(--color-muted-foreground)]">
                          {f.invoiceDate ? f.invoiceDate.slice(0, 10) : "—"}
                        </span>
                        <span className="text-[12px] tabular-nums text-[var(--color-muted-foreground)]">
                          {f.dueDate ? f.dueDate.slice(0, 10) : "—"}
                        </span>
                        <span className="text-right text-[12.5px] font-medium tabular-nums text-[var(--color-foreground)]">
                          {money(f.total)}
                        </span>
                        <Button
                          type="button"
                          variant="outline"
                          disabled={mutate.isPending || !item}
                          onClick={() => item && mutate.mutate({ lineId: item.id, invoiceId: f.id })}
                          className="h-7 shrink-0 rounded-md px-2.5 text-[12px]"
                        >
                          <Link2 className="mr-1 size-3" />
                          Vincular
                        </Button>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          </div>
        </DialogBody>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline">
              Cerrar
            </Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

function AmountDialog({
  open,
  item,
  kind,
  title,
  supplierOptions,
  onClose,
  onSubmit,
}: {
  open: boolean;
  item: LedgerItem | null; // null = crear; con item = editar
  kind: "income" | "payment";
  title: string;
  supplierOptions?: ComboboxOption[];
  onClose: () => void;
  onSubmit: (values: LedgerValues, item: LedgerItem | null) => Promise<unknown>;
}) {
  const isEdit = item !== null;
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [date, setDate] = useState("");
  const [percentage, setPercentage] = useState("");
  const [supplierId, setSupplierId] = useState<string | null>(null);
  const [projectId, setProjectId] = useState<string | null>(null);
  const [confirmed, setConfirmed] = useState(false);
  const [validated, setValidated] = useState(false);
  const [pending, setPending] = useState(false);

  // Proyecto solo se edita sobre un registro existente, como en la app original.
  const projectsQ = useQuery({
    queryKey: ["projects", "options"],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: open && isEdit,
  });

  useEffect(() => {
    if (open) {
      setAmount(item ? String(item.amount) : "");
      setDescription(item?.description ?? "");
      setDate(item?.date ? item.date.slice(0, 10) : "");
      setPercentage(item?.percentage != null ? String(item.percentage) : "");
      setSupplierId(item?.supplierId ?? null);
      setProjectId(item?.projectId ?? null);
      setConfirmed(item?.confirmed ?? false);
      setValidated(item?.validated ?? false);
    }
  }, [open, item]);

  const submit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const amt = toNum(amount);
    if (amt === null) return;
    setPending(true);
    onSubmit(
      {
        amount: amt,
        description: description.trim() || null,
        date: dateToIso(date),
        percentage: toNum(percentage),
        supplierId,
        projectId,
        confirmed,
        validated,
      },
      item,
    )
      .catch((err) => toast.error("Error al guardar", { description: describe(err) }))
      .finally(() => setPending(false));
  };

  return (
    <Dialog open={open} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-lg">
        <form onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>
              {isEdit ? `Editar ${kind === "income" ? "ingreso" : "pago"}` : `Crear ${kind === "income" ? "ingreso" : "pago"}`}
            </DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">
            {isEdit && (
              <Field id="a-id" label={`${title.slice(0, -1)} Id`}>
                <p className="truncate font-mono text-[12px] text-[var(--color-muted-foreground)]">{item.id}</p>
              </Field>
            )}
            <div className="grid grid-cols-2 gap-4">
              <Field id="a-amount" label="Importe" required>
                <Input id="a-amount" type="number" step="any" value={amount} onChange={(e) => setAmount(e.target.value)} placeholder="0" autoFocus required />
              </Field>
              <Field id="a-pct" label="Porcentaje">
                <Input id="a-pct" type="number" step="any" value={percentage} onChange={(e) => setPercentage(e.target.value)} placeholder="100" />
              </Field>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <Field id="a-desc" label="Descripción">
                <Input id="a-desc" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Anticipo, factura…" />
              </Field>
              <Field id="a-date" label="Fecha">
                <Input id="a-date" type="date" value={date} onChange={(e) => setDate(e.target.value)} />
              </Field>
            </div>
            {supplierOptions && (
              <Field id="a-supplier" label="Proveedor">
                <Combobox
                  id="a-supplier"
                  label="Proveedor"
                  variant="field"
                  value={supplierId}
                  onChange={setSupplierId}
                  options={supplierOptions}
                  searchable
                  clearable
                  placeholder="Sin asignar"
                  emptyOptionLabel="Sin asignar"
                />
              </Field>
            )}
            {isEdit && (
              <Field id="a-project" label="Proyecto">
                <Combobox
                  id="a-project"
                  label="Proyecto"
                  variant="field"
                  value={projectId}
                  onChange={setProjectId}
                  options={(projectsQ.data?.items ?? []).map((p) => ({ value: p.id, label: p.name }))}
                  searchable
                  clearable
                  placeholder={projectsQ.isLoading ? "Cargando…" : "Sin proyecto"}
                  emptyOptionLabel="Sin proyecto"
                />
              </Field>
            )}
            {isEdit && (
              <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2.5">
                <div className="flex items-center gap-6">
                  <label className="flex cursor-pointer items-center gap-2 text-[13px] text-[var(--color-foreground)]">
                    <input
                      type="checkbox"
                      checked={confirmed}
                      onChange={(e) => setConfirmed(e.target.checked)}
                      className="size-4 cursor-pointer accent-[var(--color-success)]"
                    />
                    Confirmado
                  </label>
                  <label className="flex cursor-pointer items-center gap-2 text-[13px] text-[var(--color-foreground)]">
                    <input
                      type="checkbox"
                      checked={validated}
                      onChange={(e) => setValidated(e.target.checked)}
                      className="size-4 cursor-pointer accent-[var(--color-info)]"
                    />
                    Validado
                  </label>
                </div>
                <EntityStatusBadge tone={validated ? "info" : confirmed ? "success" : "default"} withDot>
                  {validated ? "Validado" : confirmed ? "Confirmado" : "Pendiente"}
                </EntityStatusBadge>
              </div>
            )}
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={pending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={pending || toNum(amount) === null}>
              {pending ? "Guardando…" : isEdit ? "Actualizar" : "Guardar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ───────────────────────────────────────────────────────────────────────
//  Notes
// ───────────────────────────────────────────────────────────────────────

function NotesSection({ projectId }: { projectId: string }) {
  const queryClient = useQueryClient();
  const [adding, setAdding] = useState(false);
  const key = ["projects", "detail", projectId, "notes"];

  const query = useQuery({
    queryKey: key,
    queryFn: () => searchNotes({ projectId, pageSize: 100, sortBy: "date", sortDir: "desc" }),
    enabled: projectId.length > 0,
    placeholderData: keepPreviousData,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: key });

  const remove = useMutation({
    mutationFn: (id: string) => deleteNote(id),
    onSuccess: () => { toast.success("Nota borrada"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const items = query.data?.items ?? [];

  return (
    <section id="notas" className="overflow-hidden rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] shadow-xs">
      <div className="flex items-center justify-between border-b border-[var(--color-border)] px-5 py-3.5">
        <div className="flex items-baseline gap-2">
          <h2 className="font-display text-[15px] font-semibold text-[var(--color-foreground)]">Notas</h2>
          <span className="font-mono text-[11px] text-[var(--color-muted-foreground)]">{items.length}</span>
        </div>
        <Button variant="soft" onClick={() => setAdding(true)} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
          <Plus className="size-3.5" />
          Añadir
        </Button>
      </div>

      {items.length === 0 ? (
        <p className="px-5 py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">
          {query.isLoading ? "Cargando…" : "Sin notas todavía."}
        </p>
      ) : (
        <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.4)]">
          {items.map((n) => (
            <li key={n.id} className="flex items-start gap-3 px-5 py-3">
              <StickyNote className="mt-0.5 size-4 shrink-0 text-[var(--color-muted-foreground)]" />
              <div className="min-w-0 flex-1">
                <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{n.title}</p>
                {n.description && <p className="mt-0.5 text-[12.5px] text-[var(--color-muted-foreground)]">{n.description}</p>}
              </div>
              <button
                type="button"
                title="Borrar"
                aria-label="Borrar nota"
                disabled={remove.isPending}
                onClick={() => remove.mutate(n.id)}
                className="grid size-7 shrink-0 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
              >
                <Trash2 className="size-3.5" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <AddNoteDialog
        open={adding}
        onClose={() => setAdding(false)}
        onSubmit={(title, description) =>
          createNote({ projectId, title, description }).then(() => {
            toast.success("Nota creada");
            invalidate();
            setAdding(false);
          })
        }
      />
    </section>
  );
}

function AddNoteDialog({
  open,
  onClose,
  onSubmit,
}: {
  open: boolean;
  onClose: () => void;
  onSubmit: (title: string, description: string | null) => Promise<unknown>;
}) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [pending, setPending] = useState(false);

  useEffect(() => {
    if (open) {
      setTitle("");
      setDescription("");
    }
  }, [open]);

  const submit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const t = title.trim();
    if (!t) return;
    setPending(true);
    onSubmit(t, description.trim() || null)
      .catch((err) => toast.error("Error al crear", { description: describe(err) }))
      .finally(() => setPending(false));
  };

  return (
    <Dialog open={open} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <form onSubmit={submit}>
          <DialogHeader>
            <DialogTitle>Añadir nota</DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <Field id="n-title" label="Título" required>
              <Input id="n-title" value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Recordatorio…" autoFocus required />
            </Field>
            <Field id="n-desc" label="Descripción">
              <Input id="n-desc" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Detalle de la nota" />
            </Field>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={pending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={pending || !title.trim()}>
              {pending ? "Guardando…" : "Añadir"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
