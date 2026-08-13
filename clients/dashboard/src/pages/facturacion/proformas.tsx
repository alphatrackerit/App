import { useEffect, useRef, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ArrowUpRight,
  Download,
  ExternalLink,
  FileSpreadsheet,
  FileText,
  Layers,
  Loader2,
  Pencil,
  Plus,
  ScanLine,
  Search,
  Trash2,
} from "lucide-react";
import { toast } from "sonner";
import {
  attachProformaDocument,
  createProforma,
  deleteProforma,
  downloadProformaPdf,
  generateInvoicesFromProforma,
  getProformaDocumentUrl,
  getProformaLines,
  linkInvoiceToProforma,
  searchProformas,
  updateProforma,
  type ProformaInput,
  type ProformaPendingFilter,
  type ProformaRow,
} from "@/api/proformas";
import { createFactura, type FacturaType } from "@/api/facturas";
import { searchProjects } from "@/api/projects";
import { clientsApi, companiesApi, societiesApi, statusesApi, suppliersApi } from "@/api/administration";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogBody,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import {
  Combobox,
  EntityColHeader,
  EntityEmpty,
  EntityFilterEmptyRow,
  EntityFilterPill,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityMobileCard,
  EntityPageHeader,
  EntityPager,
  EntitySearch,
  EntityStatusBadge,
  Field,
  useTableState,
  useTableRows,
  type ComboboxOption,
} from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { PAYMENT_TERMS_HELP, isValidPaymentTerms } from "@/lib/payment-terms";

const PAGE_SIZE = 20;
const cols =
  "grid-cols-[minmax(0,1.1fr)_82px_minmax(0,1fr)_minmax(0,0.9fr)_minmax(0,0.8fr)_92px_100px_100px_100px_116px]";

const fmtMoney = (n: number) => new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n);
const nn = (s: string) => s.trim() || null;
const isoToDate = (iso: string | null) => (iso ? iso.slice(0, 10) : "");
const dateToIso = (d: string) => (d ? `${d}T00:00:00Z` : null);
const toNumN = (s: string): number | null => {
  const t = s.trim();
  if (!t) return null;
  const n = Number.parseFloat(t.replace(",", "."));
  return Number.isNaN(n) ? null : n;
};

function toOptions(items: { id: string; name: string; code?: string | null }[] | undefined): ComboboxOption[] {
  return (items ?? []).map((i) => ({ value: i.id, label: i.name, hint: i.code ?? undefined }));
}

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; item: ProformaRow }
  | { mode: "delete"; item: ProformaRow }
  | { mode: "facturas"; item: ProformaRow };

const TYPE_OPTIONS: { value: FacturaType; label: string }[] = [
  { value: "Emitida", label: "Emitida" },
  { value: "Recibida", label: "Recibida" },
];

// ══════════════════════ Page shell ══════════════════════

export function ProformasPage() {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  // Sort/filtro por columna + paginación. Con un orden o filtro activo la
  // consulta se ensancha al dataset completo y la tabla pagina en memoria.
  const ctl = useTableState({ pageSize: PAGE_SIZE });
  const { setPage } = ctl;
  const [editor, setEditor] = useState<EditorState>({ mode: "closed" });
  // Filtro servidor de trabajo pendiente (sin factura / facturas sin número / parcial).
  const [pending, setPending] = useState<ProformaPendingFilter | null>(null);

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPage(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search, setPage]);

  const q = useQuery({
    queryKey: ["proformas", { search: debouncedSearch, pending, ...ctl.fetch }],
    queryFn: () =>
      searchProformas({ search: debouncedSearch || undefined, pending: pending ?? undefined, ...ctl.fetch, sortDir: "desc" }),
    placeholderData: keepPreviousData,
  });

  const clientsQ = useQuery({
    queryKey: ["administration", "clients", "options"],
    queryFn: () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });
  const suppliersQ = useQuery({
    queryKey: ["administration", "suppliers", "options"],
    queryFn: () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });
  const companiesQ = useQuery({
    queryKey: ["administration", "companies", "options"],
    queryFn: () => companiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });
  const nameOf = (items: { id: string; name: string }[] | undefined, id: string | null): string =>
    (id && items?.find((i) => i.id === id)?.name) || "—";
  const counterparty = (it: ProformaRow): string =>
    it.type === "Emitida" ? nameOf(clientsQ.data?.items, it.clientId) : nameOf(suppliersQ.data?.items, it.supplierId);

  const pdf = useMutation({
    mutationFn: (it: ProformaRow) => downloadProformaPdf(it.id, it.number),
    onError: (err) => toast.error("No se pudo descargar el PDF", { description: describe(err) }),
  });

  const data = q.data;
  const view = useTableRows(
    data?.items ?? [],
    {
      numero: (it) => it.number,
      tipo: (it) => it.type,
      contraparte: (it) => counterparty(it),
      empresa: (it) => nameOf(companiesQ.data?.items, it.companyId),
      responsable: (it) => it.responsible,
      fecha: (it) => it.date,
      total: (it) => it.total,
      facturado: (it) => it.invoiced,
      pendiente: (it) => it.total - it.invoiced,
    },
    ctl,
    data,
  );
  const items = view.rows;
  const searchActive = debouncedSearch.length > 0;
  const showEmpty = items.length === 0 && !ctl.hasActiveFilters && pending === null;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={FileSpreadsheet}
        title="Proformas"
        total={view.totalCount}
        unit="proforma"
        description="Documentos comerciales previos a la factura: 1 proforma genera N facturas."
      >
        <Button
          onClick={() => setEditor({ mode: "create" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Plus className="size-4" />
          Nueva
        </Button>
      </EntityPageHeader>

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por número, cliente, proveedor, empresa o importe…" />

      <EntityFilterPill<ProformaPendingFilter | null>
        label="Pendientes de facturación"
        value={pending}
        onChange={(next) => {
          setPending(next);
          setPage(1);
        }}
        options={[
          { value: null, label: "Todas" },
          { value: "SinFactura", label: "Sin factura" },
          { value: "FacturasSinNumero", label: "Facturas sin número" },
          { value: "ParcialmenteFacturada", label: "Parcialmente facturadas" },
        ]}
      />

      {q.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={cols} />
      ) : showEmpty ? (
        <EntityEmpty
          icon={searchActive ? Search : FileSpreadsheet}
          title={searchActive ? "Sin resultados" : "Aún no hay proformas"}
          body={searchActive ? `Nada coincide con "${debouncedSearch}".` : "Crea la primera proforma para empezar."}
          action={
            <Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]">
              <Plus className="mr-1.5 size-4" />
              Nueva
            </Button>
          }
        />
      ) : (
        <div>
          <div className="space-y-2 md:hidden">
            {items.map((it) => (
              <EntityMobileCard
                key={it.id}
                href="#"
                onClick={(e) => {
                  e.preventDefault();
                  setEditor({ mode: "edit", item: it });
                }}
                aria-label={`Editar proforma ${it.number}`}
              >
                <div className="min-w-0">
                  <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.number || "—"}</p>
                  <p className="truncate text-[12px] text-[var(--color-muted-foreground)]">
                    {`${it.type} · ${counterparty(it)} · ${it.date ? it.date.slice(0, 10) : "—"} · ${fmtMoney(it.total)}`}
                  </p>
                </div>
              </EntityMobileCard>
            ))}
          </div>

          <EntityListCard className="hidden md:block">
            <EntityListHeader className={cols}>
              {(
                [
                  ["numero", "Número", undefined],
                  ["tipo", "Tipo", undefined],
                  ["contraparte", "Cliente / Proveedor", undefined],
                  ["empresa", "Empresa", undefined],
                  ["responsable", "Responsable", undefined],
                  ["fecha", "Fecha", undefined],
                  ["total", "Total", "right"],
                  ["facturado", "Facturado", "right"],
                  ["pendiente", "Pendiente", "right"],
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
            {items.length === 0 && <EntityFilterEmptyRow onClear={ctl.clearFilters} />}
            {items.map((it, i) => {
              const pendiente = it.total - it.invoiced;
              return (
                <EntityListRow key={it.id} className={cols} isLast={i === items.length - 1}>
                  <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.number || "—"}</span>
                  <span>
                    <EntityStatusBadge tone={it.type === "Emitida" ? "info" : "default"}>{it.type}</EntityStatusBadge>
                  </span>
                  <span className="truncate text-[13px] text-[var(--color-muted-foreground)]" title={counterparty(it)}>
                    {counterparty(it)}
                  </span>
                  <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">
                    {nameOf(companiesQ.data?.items, it.companyId)}
                  </span>
                  <span className="truncate text-[13px] text-[var(--color-muted-foreground)]" title={it.responsible ?? undefined}>
                    {it.responsible || "—"}
                  </span>
                  <span className="text-[13px] tabular-nums text-[var(--color-muted-foreground)]">
                    {it.date ? it.date.slice(0, 10) : "—"}
                  </span>
                  <span className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmtMoney(it.total)}</span>
                  <span className="text-right text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{fmtMoney(it.invoiced)}</span>
                  <span
                    className={
                      "text-right text-[13px] tabular-nums " +
                      (pendiente < 0 ? "text-[var(--color-destructive)]" : "text-[var(--color-muted-foreground)]")
                    }
                    title={pendiente < 0 ? "Facturado por encima del total (exceso)" : undefined}
                  >
                    {fmtMoney(pendiente)}
                  </span>
                  <div className="flex items-center justify-end gap-1">
                    <button
                      type="button"
                      aria-label={`Facturas de la proforma ${it.number}`}
                      onClick={() => setEditor({ mode: "facturas", item: it })}
                      className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)] group-hover:opacity-100"
                    >
                      <Layers className="size-3.5" />
                    </button>
                    <button
                      type="button"
                      aria-label={`Editar proforma ${it.number}`}
                      onClick={() => setEditor({ mode: "edit", item: it })}
                      className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"
                    >
                      <Pencil className="size-3.5" />
                    </button>
                    <button
                      type="button"
                      aria-label={`Descargar PDF de la proforma ${it.number}`}
                      disabled={pdf.isPending}
                      onClick={() => pdf.mutate(it)}
                      className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)] disabled:opacity-40 group-hover:opacity-100"
                    >
                      <Download className="size-3.5" />
                    </button>
                    <button
                      type="button"
                      aria-label={`Borrar proforma ${it.number}`}
                      onClick={() => setEditor({ mode: "delete", item: it })}
                      className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)] group-hover:opacity-100"
                    >
                      <Trash2 className="size-3.5" />
                    </button>
                  </div>
                </EntityListRow>
              );
            })}
          </EntityListCard>

          <EntityPager
            page={ctl.page}
            totalPages={view.totalPages}
            hasPrev={view.hasPrev}
            hasNext={view.hasNext}
            onPrev={() => setPage(ctl.page - 1)}
            onNext={() => setPage(ctl.page + 1)}
            pageSize={ctl.pageSize}
            onPageSizeChange={ctl.setPageSize}
          />
        </div>
      )}

      {q.isError && (
        <div
          role="alert"
          className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]"
        >
          {describe(q.error)}
        </div>
      )}

      <ProformaEditor state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <DeleteDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <FacturasDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

// ══════════════════════ Facturas vinculadas + creación/generación ══════════════════════
// Tres vías de asociación (una proforma se paga con 1..N facturas):
//  1. "Crear factura": alta directa en borrador con los datos de la proforma y el importe elegido
//     (por defecto, el pendiente) — el caso "llega la factura que paga la proforma".
//  2. "Generar facturas": una factura por plazo de la FormaPago (restaurada 2026-08-03).
//  3. Manual: desde el editor de la factura, selector «Proforma».

function FacturasDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "facturas";
  const item = isOpen ? state.item : undefined;
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  // Acceso directo a la factura: Facturas la abre por ?factura=<id> aunque esté
  // en otra página del listado. Cerramos antes para no dejar el diálogo montado.
  const openInvoice = (invoiceId: string) => {
    onClose();
    navigate(`/facturacion/facturas?factura=${encodeURIComponent(invoiceId)}`);
  };
  // Doble paso cuando ya hay facturas: primer clic arma la confirmación, el segundo genera.
  const [confirmArmed, setConfirmArmed] = useState(false);
  const [createAmount, setCreateAmount] = useState("");
  useEffect(() => {
    if (isOpen) {
      setConfirmArmed(false);
      setCreateAmount("");
    }
  }, [isOpen]);

  const linesQ = useQuery({
    queryKey: ["proformas", state.mode === "facturas" ? state.item.id : "none", "lines"],
    queryFn: () => getProformaLines((state as { item: ProformaRow }).item.id),
    enabled: isOpen,
  });

  const lines = linesQ.data;
  const hasInvoices = (lines?.invoices.length ?? 0) > 0;
  // Importe propuesto: lo pendiente (o el total si aún no hay nada facturado).
  const pendingSuggestion = lines && lines.pending > 0 ? lines.pending : null;

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ["proformas"] });
    queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
    queryClient.invalidateQueries({ queryKey: ["facturacion"] });
  };

  const generate = useMutation({
    mutationFn: (id: string) => generateInvoicesFromProforma(id),
    onSuccess: (ids) => {
      toast.success(`${ids.length} factura(s) generadas en borrador`);
      setConfirmArmed(false);
      invalidateAll();
    },
    onError: (err) => toast.error("No se pudieron generar las facturas", { description: describe(err) }),
  });

  // Alta directa: borrador con los datos de la proforma (base/IVA proporcionales al importe),
  // numerada {NumProforma}-{n} con el primer sufijo libre, y vinculada al momento.
  const createOne = useMutation({
    mutationFn: async (vars: { proforma: ProformaRow; amount: number }) => {
      const { proforma, amount } = vars;
      // Recibidas nacen sin número (borrador hasta que llegue la factura del proveedor);
      // Emitidas se numeran {NumProforma}-{n} con el primer sufijo libre.
      let number: string | null = null;
      if (proforma.type === "Emitida") {
        const existing = new Set((lines?.invoices ?? []).map((i) => i.number));
        let n = (lines?.invoices.length ?? 0) + 1;
        while (existing.has(`${proforma.number}-${n}`)) n++;
        number = `${proforma.number}-${n}`;
      }
      const ratio = proforma.total !== 0 ? amount / proforma.total : 1;
      const round2 = (x: number) => Math.round(x * 100) / 100;
      const invoiceId = await createFactura({
        type: proforma.type,
        number,
        total: round2(amount),
        clientId: proforma.type === "Emitida" ? proforma.clientId : null,
        supplierId: proforma.type === "Recibida" ? proforma.supplierId : null,
        invoiceDate: `${new Date().toISOString().slice(0, 10)}T00:00:00Z`,
        companyId: proforma.companyId,
        societyId: proforma.societyId,
        projectId: proforma.projectId,
        taxBase: proforma.taxBase != null ? round2(proforma.taxBase * ratio) : null,
        vat: proforma.vat != null ? round2(proforma.vat * ratio) : null,
        paymentTerms: proforma.paymentTerms,
      });
      await linkInvoiceToProforma({ invoiceId, proformaId: proforma.id });
      return invoiceId;
    },
    onSuccess: () => {
      toast.success("Factura creada en borrador y vinculada", { description: "Edítala en Facturas si necesitas ajustar número o fechas." });
      setCreateAmount("");
      invalidateAll();
    },
    onError: (err) => toast.error("No se pudo crear la factura", { description: describe(err) }),
  });

  const onGenerate = () => {
    if (!item) return;
    if (hasInvoices && !confirmArmed) {
      setConfirmArmed(true);
      return;
    }
    generate.mutate(item.id);
  };

  const amountToCreate = toNumN(createAmount) ?? pendingSuggestion;

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-2xl">
        <DialogHeader>
          <DialogTitle>Facturas de la proforma {item?.number}</DialogTitle>
          <DialogDescription>
            Cuadre informativo — el exceso o defecto de facturación nunca bloquea.
          </DialogDescription>
        </DialogHeader>
        <DialogBody className="space-y-4">
          {linesQ.isLoading ? (
            <p className="py-8 text-center text-[13px] text-[var(--color-muted-foreground)]">Cargando…</p>
          ) : linesQ.isError ? (
            <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] px-3 py-2 text-sm text-[var(--color-destructive)]">
              {describe(linesQ.error)}
            </div>
          ) : lines ? (
            <>
              <div className="grid grid-cols-3 gap-3">
                {(
                  [
                    ["Total proforma", lines.total, false],
                    ["Facturado", lines.invoicedAmount, false],
                    [lines.pending < 0 ? "Exceso" : "Pendiente", Math.abs(lines.pending), lines.pending < 0],
                  ] as const
                ).map(([label, value, danger]) => (
                  <div key={label} className="rounded-lg border border-[var(--color-border)] px-3 py-2.5">
                    <p className="text-[11px] font-medium uppercase tracking-wider text-[var(--color-muted-foreground)]">{label}</p>
                    <p className={"mt-0.5 text-[15px] font-semibold tabular-nums " + (danger ? "text-[var(--color-destructive)]" : "text-[var(--color-foreground)]")}>
                      {fmtMoney(value)}
                    </p>
                  </div>
                ))}
              </div>

              {lines.invoices.length === 0 ? (
                <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-6 text-center text-[13px] text-[var(--color-muted-foreground)]">
                  Sin facturas vinculadas todavía.
                  <span className="mt-1 block text-[12px]">
                    Crea una aquí con los datos de la proforma, genera una por plazo, o asóciala a
                    mano desde Facturas → editar → selector «Proforma».
                  </span>
                </p>
              ) : (
                <div className="overflow-x-auto rounded-lg border border-[var(--color-border)]">
                  <table className="w-full min-w-[420px]">
                    <thead>
                      <tr className="border-b border-[var(--color-border)] text-left text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
                        <th className="px-2.5 py-1.5">Número</th>
                        <th className="px-2.5 py-1.5">Fecha</th>
                        <th className="px-2.5 py-1.5">Vencimiento</th>
                        <th className="px-2.5 py-1.5 text-right">Total</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-[var(--color-border)]">
                      {lines.invoices.map((inv) => (
                        <tr key={inv.id}>
                          <td className="px-2.5 py-1.5 text-[12.5px] font-medium text-[var(--color-foreground)]">
                            <button
                              type="button"
                              onClick={() => openInvoice(inv.id)}
                              title="Abrir la factura en Facturas"
                              aria-label={`Abrir la factura ${inv.number ?? "(sin número)"}`}
                              className="group flex cursor-pointer items-center gap-1.5 rounded text-left text-[var(--color-foreground)] transition-colors hover:text-[var(--color-primary)]"
                            >
                              <FileText className="size-3.5 shrink-0 text-[var(--color-muted-foreground)] group-hover:text-[var(--color-primary)]" />
                              <span className={"underline decoration-dotted underline-offset-2" + (inv.number ? "" : " italic text-[var(--color-muted-foreground)]")}>
                                {inv.number ?? "(sin número)"}
                              </span>
                              <ArrowUpRight className="size-3 shrink-0 opacity-0 transition-opacity group-hover:opacity-100" />
                            </button>
                          </td>
                          <td className="px-2.5 py-1.5 text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">
                            {inv.invoiceDate ? inv.invoiceDate.slice(0, 10) : "—"}
                          </td>
                          <td className="px-2.5 py-1.5 text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">
                            {inv.dueDate ? inv.dueDate.slice(0, 10) : "—"}
                          </td>
                          <td className="px-2.5 py-1.5 text-right text-[12.5px] font-medium tabular-nums text-[var(--color-foreground)]">
                            {fmtMoney(inv.total)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}

              {/* ── Crear factura con los datos de la proforma ── */}
              {item && (
                <div className="flex flex-wrap items-end gap-2 rounded-lg border border-[var(--color-border)] p-3">
                  <div className="min-w-0 flex-1">
                    <p className="text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">
                      Crear factura desde la proforma
                    </p>
                    <p className="mt-0.5 text-[12px] text-[var(--color-muted-foreground)]">
                      Borrador con {item.type === "Emitida" ? "el cliente" : "el proveedor"}, empresa,
                      proyecto y base/IVA proporcionales;{" "}
                      {item.type === "Emitida"
                        ? `se numera ${item.number}-n y queda vinculada.`
                        : "queda sin número (ponlo al recibir la factura) y vinculada."}
                    </p>
                  </div>
                  <div className="w-32">
                    <Input
                      type="number"
                      step="0.01"
                      value={createAmount}
                      onChange={(e) => setCreateAmount(e.target.value)}
                      placeholder={pendingSuggestion != null ? fmtMoney(pendingSuggestion) : "Importe"}
                      aria-label="Importe de la factura a crear"
                    />
                  </div>
                  <Button
                    type="button"
                    variant="outline"
                    disabled={createOne.isPending || !amountToCreate || amountToCreate <= 0}
                    title={!amountToCreate ? "Indica un importe (por defecto, el pendiente)." : undefined}
                    onClick={() => amountToCreate && createOne.mutate({ proforma: item, amount: amountToCreate })}
                    className="gap-1.5"
                  >
                    {createOne.isPending ? (
                      <>
                        <Loader2 className="size-3.5 animate-spin" />
                        Creando…
                      </>
                    ) : (
                      <>
                        <Plus className="size-3.5" />
                        Crear factura
                      </>
                    )}
                  </Button>
                </div>
              )}

              {confirmArmed && (
                <p className="rounded-lg border border-[oklch(from_var(--color-warning)_l_c_h_/_0.35)] bg-[oklch(from_var(--color-warning)_l_c_h_/_0.08)] px-3 py-2 text-[12.5px] text-[var(--color-foreground)]">
                  Ya hay {lines.invoices.length} factura(s) vinculadas. Generar de nuevo creará facturas
                  adicionales (una por plazo de la forma de pago). Pulsa otra vez para confirmar.
                </p>
              )}
            </>
          ) : null}
        </DialogBody>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={generate.isPending}>
              Cerrar
            </Button>
          </DialogClose>
          <Button
            onClick={onGenerate}
            disabled={generate.isPending || !item || !item.paymentTerms}
            title={!item?.paymentTerms ? "La proforma no tiene forma de pago (FormaPago)." : undefined}
          >
            {generate.isPending ? (
              <>
                <Loader2 className="mr-1.5 size-4 animate-spin" />
                Generando…
              </>
            ) : confirmArmed ? (
              "Confirmar generación"
            ) : (
              "Generar facturas"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ══════════════════════ Borrado ══════════════════════

function DeleteDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const item = isOpen ? state.item : undefined;
  const queryClient = useQueryClient();
  const del = useMutation({
    mutationFn: (id: string) => deleteProforma(id),
    onSuccess: () => {
      toast.success("Proforma borrada");
      queryClient.invalidateQueries({ queryKey: ["proformas"] });
      queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
      onClose();
    },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar proforma</DialogTitle>
          <DialogDescription>
            Esto elimina permanentemente la proforma{" "}
            <span className="font-medium text-[var(--color-foreground)]">{item?.number}</span>. Las facturas
            vinculadas no se borran — quedan desvinculadas.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={del.isPending}>
              Cancelar
            </Button>
          </DialogClose>
          <Button variant="destructive" onClick={() => item && del.mutate(item.id)} disabled={del.isPending || !item}>
            {del.isPending ? "Borrando…" : "Borrar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ══════════════════════ Editor ══════════════════════

type ProformaForm = {
  type: FacturaType;
  number: string;
  total: string;
  taxBase: string;
  vat: string;
  date: string;
  paymentTerms: string;
  responsible: string;
  notes: string;
  clientId: string | null;
  supplierId: string | null;
  companyId: string | null;
  societyId: string | null;
  projectId: string | null;
  statusId: string | null;
  documentPath: string | null;
};

const BLANK: ProformaForm = {
  type: "Emitida",
  number: "",
  total: "",
  taxBase: "",
  vat: "",
  date: "",
  paymentTerms: "",
  responsible: "",
  notes: "",
  clientId: null,
  supplierId: null,
  companyId: null,
  societyId: null,
  projectId: null,
  statusId: null,
  documentPath: null,
};

function useLookupOptions(queryKey: string, loader: () => Promise<{ items: { id: string; name: string; code?: string | null }[] }>, enabled: boolean) {
  return useQuery({
    queryKey: ["administration", queryKey, "options"],
    queryFn: loader,
    enabled,
  });
}

function ProformaEditor({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [f, setF] = useState<ProformaForm>(BLANK);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (isOpen) {
      setF(
        item
          ? {
              type: item.type,
              number: item.number,
              total: item.total != null ? String(item.total) : "",
              taxBase: item.taxBase != null ? String(item.taxBase) : "",
              vat: item.vat != null ? String(item.vat) : "",
              date: isoToDate(item.date),
              paymentTerms: item.paymentTerms ?? "",
              responsible: item.responsible ?? "",
              notes: item.notes ?? "",
              clientId: item.clientId,
              supplierId: item.supplierId,
              companyId: item.companyId,
              societyId: item.societyId,
              projectId: item.projectId,
              statusId: item.statusId,
              documentPath: item.documentPath,
            }
          : BLANK,
      );
    }
  }, [isOpen, item]);

  const clientsQ = useLookupOptions("clients", () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const suppliersQ = useLookupOptions("suppliers", () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const companiesQ = useLookupOptions("companies", () => companiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const societiesQ = useLookupOptions("societies", () => societiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  // El catálogo de estados es polimórfico: cada formulario solo debe ver los de su contexto
  // (ProformaEmitida / ProformaRecibida), nunca los de proyectos, facturas o movimientos.
  const statusType = f.type === "Emitida" ? "ProformaEmitida" : "ProformaRecibida";
  const statusesQ = useQuery({
    queryKey: ["administration", "statuses", "options", statusType],
    queryFn: () => statusesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc", type: statusType }),
    enabled: isOpen,
  });
  const projectsQ = useQuery({
    queryKey: ["projects", "options"],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen,
  });

  const openDocument = useMutation({
    mutationFn: (id: string) => getProformaDocumentUrl(id),
    onSuccess: ({ url }) => window.open(url, "_blank", "noopener"),
    onError: (err) => toast.error("No se pudo abrir el documento", { description: describe(err) }),
  });

  const attachInputRef = useRef<HTMLInputElement>(null);
  const attach = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => attachProformaDocument(id, file),
    onSuccess: ({ documentPath }) => {
      setF((s) => ({ ...s, documentPath }));
      queryClient.invalidateQueries({ queryKey: ["proformas"] });
      toast.success("Documento adjuntado a la proforma");
    },
    onError: (err) => toast.error("No se pudo adjuntar el documento", { description: describe(err) }),
  });

  const save = useMutation({
    mutationFn: async (input: ProformaInput) => {
      if (item) return updateProforma(item.id, input);
      return createProforma(input);
    },
    onSuccess: () => {
      toast.success(item ? "Actualizada" : "Creada");
      queryClient.invalidateQueries({ queryKey: ["proformas"] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const set = <K extends keyof ProformaForm>(k: K, v: ProformaForm[K]) => setF((s) => ({ ...s, [k]: v }));

  // Base/IVA/Total consistentes (21 % por defecto) — mismo comportamiento que el editor de facturas.
  const num = (s: string): number => {
    const n = Number(s.replace(",", "."));
    return Number.isFinite(n) ? n : 0;
  };
  const fmt2 = (n: number): string => String(Math.round(n * 100) / 100);
  // vatRateText es de UI (admite tipos personalizados, p.ej. 7,5); vatRate es el número derivado.
  const [vatRateText, setVatRateText] = useState("21");
  const vatRate = num(vatRateText);
  useEffect(() => {
    if (isOpen) setVatRateText("21");
  }, [isOpen]);
  const onVatRateChange = (rateText: string) => {
    setVatRateText(rateText);
    const rate = num(rateText);
    setF((s) => {
      if (s.taxBase.trim() === "" && s.total.trim() === "") return s;
      const vat = fmt2(num(s.taxBase) * (rate / 100));
      return { ...s, vat, total: fmt2(num(s.taxBase) + num(vat)) };
    });
  };
  const setAmount = (k: "taxBase" | "vat" | "total", v: string, rate = vatRate) =>
    setF((s) => {
      const next = { ...s, [k]: v };
      if (k === "total") {
        const base = num(next.total) / (1 + rate / 100);
        next.taxBase = fmt2(base);
        next.vat = fmt2(num(next.total) - num(fmt2(base)));
      } else if (k === "taxBase") {
        next.vat = fmt2(num(next.taxBase) * (rate / 100));
        next.total = fmt2(num(next.taxBase) + num(next.vat));
      } else {
        next.total = fmt2(num(next.taxBase) + num(next.vat));
      }
      return next;
    });

  const trimmedNumber = f.number.trim();
  const totalNum = toNumN(f.total);
  // El servidor exige contraparte al crear (Emitida→cliente, Recibida→proveedor)
  // y solo acepta códigos de forma de pago de su gramática: validamos aquí para
  // no mandar la petición y comerse un 400 sin explicación. Al editar no se exige
  // —igual que el validador de Update— para no bloquear datos heredados sin cliente.
  const counterpartyOk = !!item || (f.type === "Emitida" ? !!f.clientId : !!f.supplierId);
  const termsRaw = f.paymentTerms.trim();
  const termsOk = termsRaw === "" || isValidPaymentTerms(termsRaw);
  const canSave = trimmedNumber.length > 0 && totalNum !== null && counterpartyOk && termsOk;
  const blockedReason = !trimmedNumber
    ? "Falta el número."
    : totalNum === null
      ? "Falta el total."
      : !counterpartyOk
        ? f.type === "Emitida"
          ? "Selecciona un cliente."
          : "Selecciona un proveedor."
        : !termsOk
          ? "Revisa la forma de pago."
          : undefined;

  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!canSave) return;
    save.mutate({
      type: f.type,
      number: trimmedNumber,
      total: totalNum ?? 0,
      clientId: f.type === "Emitida" ? f.clientId : null,
      supplierId: f.type === "Recibida" ? f.supplierId : null,
      date: dateToIso(f.date),
      companyId: f.companyId,
      societyId: f.societyId,
      projectId: f.projectId,
      taxBase: toNumN(f.taxBase),
      vat: toNumN(f.vat),
      paymentTerms: nn(f.paymentTerms),
      statusId: f.statusId,
      responsible: nn(f.responsible),
      notes: nn(f.notes),
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md sm:!max-w-5xl">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{item ? "Editar proforma" : "Nueva proforma"}</DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">
            {item && (
              <div className="space-y-1.5">
                {f.documentPath ? (
                  <div className="flex items-stretch gap-2">
                    <button
                      type="button"
                      onClick={() => openDocument.mutate(item.id)}
                      disabled={openDocument.isPending}
                      className="flex min-w-0 flex-1 cursor-pointer items-center gap-2 rounded-lg border border-[var(--color-border)] px-3 py-2 text-[13px] text-[var(--color-foreground)] transition-colors hover:border-[var(--color-primary)] disabled:opacity-50"
                    >
                      <FileText className="size-4 shrink-0 text-[var(--color-primary)]" />
                      Ver documento adjunto
                      <ExternalLink className="ml-auto size-3.5 shrink-0 text-[var(--color-muted-foreground)]" />
                    </button>
                    <button
                      type="button"
                      title="Reemplazar documento"
                      onClick={() => !attach.isPending && attachInputRef.current?.click()}
                      disabled={attach.isPending}
                      className="flex cursor-pointer items-center gap-1.5 rounded-lg border border-[var(--color-border)] px-3 text-[12px] text-[var(--color-muted-foreground)] transition-colors hover:border-[var(--color-primary)] hover:text-[var(--color-foreground)] disabled:opacity-50"
                    >
                      {attach.isPending ? <Loader2 className="size-3.5 animate-spin" /> : <ScanLine className="size-3.5" />}
                      Reemplazar
                    </button>
                  </div>
                ) : (
                  <button
                    type="button"
                    onClick={() => !attach.isPending && attachInputRef.current?.click()}
                    disabled={attach.isPending}
                    className="flex w-full cursor-pointer items-center gap-3 rounded-lg border border-dashed border-[var(--color-border)] px-3 py-3 text-left transition-colors hover:border-[var(--color-primary)] disabled:opacity-50"
                  >
                    {attach.isPending ? (
                      <Loader2 className="size-5 shrink-0 animate-spin text-[var(--color-primary)]" />
                    ) : (
                      <ScanLine className="size-5 shrink-0 text-[var(--color-primary)]" />
                    )}
                    <span className="min-w-0">
                      <span className="block text-[13px] font-medium text-[var(--color-foreground)]">
                        {attach.isPending ? "Subiendo documento…" : "Adjuntar documento"}
                      </span>
                      <span className="block text-[11px] text-[var(--color-muted-foreground)]">
                        Esta proforma aún no tiene documento. PDF o imagen, máx. 10 MB.
                      </span>
                    </span>
                  </button>
                )}
                <input
                  ref={attachInputRef}
                  type="file"
                  accept="application/pdf,image/png,image/jpeg,image/webp,image/gif"
                  className="hidden"
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    if (file) attach.mutate({ id: item.id, file });
                    e.target.value = "";
                  }}
                />
              </div>
            )}

            <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
              <Field id="pf-type" label="Tipo" required hint={item ? "El tipo no se puede cambiar." : undefined}>
                <Combobox
                  id="pf-type"
                  label="Tipo"
                  variant="field"
                  value={f.type}
                  onChange={(v) =>
                    // El estado pertenece al tipo: al cambiarlo, el anterior deja de ser válido.
                    setF((s) => ({ ...s, type: (v as FacturaType) ?? "Emitida", clientId: null, supplierId: null, statusId: null }))
                  }
                  options={TYPE_OPTIONS.map((t) => ({ value: t.value, label: t.label }))}
                  placeholder="Tipo"
                  disabled={!!item}
                />
              </Field>
              <Field id="pf-number" label="Número" required hint="Las facturas generadas se numeran Nº-1, Nº-2…">
                <Input id="pf-number" value={f.number} onChange={(e) => set("number", e.target.value)} placeholder="Nº proforma" autoFocus required maxLength={64} />
              </Field>
              <Field id="pf-date" label="Fecha">
                <Input id="pf-date" type="date" value={f.date} onChange={(e) => set("date", e.target.value)} />
              </Field>
              <Field id="pf-status" label="Estado">
                <Combobox
                  id="pf-status"
                  label="Estado"
                  variant="field"
                  value={f.statusId}
                  onChange={(v) => set("statusId", v)}
                  options={toOptions(statusesQ.data?.items)}
                  searchable
                  clearable
                  placeholder={statusesQ.isLoading ? "Cargando…" : "Sin asignar"}
                  emptyOptionLabel="Sin asignar"
                />
              </Field>

              {f.type === "Emitida" ? (
                <Field id="pf-client" label="Cliente" required={!item}>
                  <Combobox
                    id="pf-client"
                    label="Cliente"
                    variant="field"
                    value={f.clientId}
                    onChange={(v) => set("clientId", v)}
                    options={toOptions(clientsQ.data?.items)}
                    searchable
                    clearable
                    placeholder={clientsQ.isLoading ? "Cargando…" : "Sin asignar"}
                    emptyOptionLabel="Sin asignar"
                    emptyMessage="No hay clientes dados de alta."
                  />
                </Field>
              ) : (
                <Field id="pf-supplier" label="Proveedor" required={!item}>
                  <Combobox
                    id="pf-supplier"
                    label="Proveedor"
                    variant="field"
                    value={f.supplierId}
                    onChange={(v) => set("supplierId", v)}
                    options={toOptions(suppliersQ.data?.items)}
                    searchable
                    clearable
                    placeholder={suppliersQ.isLoading ? "Cargando…" : "Sin asignar"}
                    emptyOptionLabel="Sin asignar"
                    emptyMessage="No hay proveedores dados de alta."
                  />
                </Field>
              )}
              <Field id="pf-company" label="Empresa">
                <Combobox
                  id="pf-company"
                  label="Empresa"
                  variant="field"
                  value={f.companyId}
                  onChange={(v) => set("companyId", v)}
                  options={toOptions(companiesQ.data?.items)}
                  searchable
                  clearable
                  placeholder={companiesQ.isLoading ? "Cargando…" : "Sin asignar"}
                  emptyOptionLabel="Sin asignar"
                />
              </Field>
              <Field id="pf-society" label="Sociedad">
                <Combobox
                  id="pf-society"
                  label="Sociedad"
                  variant="field"
                  value={f.societyId}
                  onChange={(v) => set("societyId", v)}
                  options={toOptions(societiesQ.data?.items)}
                  searchable
                  clearable
                  placeholder={societiesQ.isLoading ? "Cargando…" : "Sin asignar"}
                  emptyOptionLabel="Sin asignar"
                />
              </Field>

              <Field id="pf-project" label="Proyecto">
                <Combobox
                  id="pf-project"
                  label="Proyecto"
                  variant="field"
                  value={f.projectId}
                  onChange={(v) => set("projectId", v)}
                  options={toOptions(projectsQ.data?.items)}
                  searchable
                  clearable
                  placeholder={projectsQ.isLoading ? "Cargando…" : "Sin asignar"}
                  emptyOptionLabel="Sin asignar"
                />
              </Field>
              <Field
                id="pf-terms"
                label="Forma de pago"
                hint={termsOk ? "Define los plazos al generar facturas (p.ej. 30PP70-60D)." : PAYMENT_TERMS_HELP}
              >
                <Input
                  id="pf-terms"
                  value={f.paymentTerms}
                  onChange={(e) => set("paymentTerms", e.target.value)}
                  placeholder="60D, 30PP70-60D…"
                  maxLength={32}
                  aria-invalid={!termsOk}
                />
              </Field>
              <Field id="pf-base" label="Base imponible">
                <Input id="pf-base" type="text" inputMode="decimal" value={f.taxBase} onChange={(e) => setAmount("taxBase", e.target.value)} placeholder="0.00" />
              </Field>
              <Field id="pf-vat" label="IVA">
                <div className="flex gap-1.5">
                  <div className="relative w-[74px] shrink-0">
                    <input
                      aria-label="Tipo de IVA"
                      type="text"
                      inputMode="decimal"
                      list="pf-vat-rate-options"
                      value={vatRateText}
                      onChange={(e) => onVatRateChange(e.target.value)}
                      placeholder="21"
                      className="h-9 w-full rounded-lg border border-[var(--color-input)] bg-transparent py-1 pl-2 pr-5 text-[13px] text-[var(--color-foreground)] outline-none focus:border-[var(--color-primary)]"
                    />
                    <span aria-hidden className="pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 text-[12px] text-[var(--color-muted-foreground)]">
                      %
                    </span>
                    <datalist id="pf-vat-rate-options">
                      <option value="21" />
                      <option value="10" />
                      <option value="4" />
                      <option value="0" />
                    </datalist>
                  </div>
                  <Input id="pf-vat" type="text" inputMode="decimal" value={f.vat} onChange={(e) => setAmount("vat", e.target.value)} placeholder="0.00" />
                </div>
              </Field>
              <Field id="pf-total" label="Total" required>
                <Input id="pf-total" type="text" inputMode="decimal" value={f.total} onChange={(e) => setAmount("total", e.target.value)} placeholder="0.00" required />
              </Field>

              <Field id="pf-responsible" label="Responsable">
                <Input id="pf-responsible" value={f.responsible} onChange={(e) => set("responsible", e.target.value)} placeholder="Opcional" maxLength={128} />
              </Field>
              <Field id="pf-notes" label="Notas" className="col-span-2 sm:col-span-3">
                <Input id="pf-notes" value={f.notes} onChange={(e) => set("notes", e.target.value)} placeholder="Opcional" maxLength={512} />
              </Field>
            </div>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={save.isPending}>
                Cancelar
              </Button>
            </DialogClose>
            <Button type="submit" disabled={!canSave || save.isPending} title={blockedReason}>
              {save.isPending ? "Guardando…" : item ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
