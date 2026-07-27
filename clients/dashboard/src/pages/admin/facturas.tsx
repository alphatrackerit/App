import { useEffect, useRef, useState, type DragEvent, type FormEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  CalendarClock,
  CheckCircle2,
  Download,
  ExternalLink,
  FileBarChart2,
  FileText,
  Link2,
  Loader2,
  Pencil,
  Plus,
  Printer,
  ScanLine,
  Search,
  ShieldCheck,
  Sparkles,
  Trash2,
  Undo2,
  Unlink,
} from "lucide-react";
import { toast } from "sonner";
import {
  applyLinkSuggestions,
  attachInvoiceDocument,
  createFactura,
  deleteFactura,
  extractInvoice,
  getFacturaLines,
  getInvoiceDocumentUrl,
  getInvoicesReport,
  getLinkSuggestions,
  linkLine,
  searchFacturas,
  updateFactura,
  verifyFactura,
  type FacturaInput,
  type FacturaRow,
  type FacturaType,
  type InvoiceLineDto,
  type InvoiceReportRow,
  type InvoiceReportScope,
  type LinkPair,
} from "@/api/facturas";
import {
  createIncome,
  createPayment,
  searchIncomes,
  searchPayments,
  searchProjects,
  updateIncome,
  updatePayment,
  validateIncome,
  validatePayment,
} from "@/api/projects";
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
import { Switch } from "@/components/ui/switch";
import {
  Combobox,
  EntityEmpty,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityMobileCard,
  EntityPageHeader,
  EntityPager,
  EntitySearch,
  EntityStatusBadge,
  EntityColHeader,
  EntityFilterEmptyRow,
  useTableControls,
  Field,
  type ComboboxOption,
} from "@/components/list";
import { describe } from "@/lib/list-helpers";

const PAGE_SIZE = 20;
const cols =
  "grid-cols-[minmax(0,1.1fr)_82px_minmax(0,1fr)_minmax(0,0.8fr)_minmax(0,0.8fr)_88px_88px_100px_100px_90px_116px]";

/** Indicador de cobro/pago: barra de progreso con lo validado sobre el total. */
function CobroBar({ total, collected }: { total: number; collected: number }) {
  const pct = total > 0 ? Math.min(100, Math.max(0, (collected / total) * 100)) : 0;
  const label = `${new Intl.NumberFormat("es-ES", { maximumFractionDigits: 0 }).format(pct)} % (${fmtMoney(collected)} de ${fmtMoney(total)})`;
  return (
    <span className="flex items-center" title={label} aria-label={label}>
      <span className="h-1.5 w-full overflow-hidden rounded-full bg-[var(--color-muted)]">
        <span
          className="block h-full rounded-full bg-[var(--color-primary)] transition-[width]"
          style={{ width: `${pct}%` }}
        />
      </span>
    </span>
  );
}

const fmtMoney = (n: number) => new Intl.NumberFormat("es-ES", { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n);

// ─── shared form helpers (mirror rich-catalog.tsx) ───
const nn = (s: string) => s.trim() || null; // "" → null for optional text fields
const isoToDate = (iso: string | null) => (iso ? iso.slice(0, 10) : ""); // ISO → yyyy-mm-dd
const dateToIso = (d: string) => (d ? `${d}T00:00:00Z` : null); // date input → ISO the backend parses
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
  | { mode: "edit"; item: FacturaRow }
  | { mode: "delete"; item: FacturaRow }
  | { mode: "lines"; item: FacturaRow }
  | { mode: "vencimientos"; item: FacturaRow }
  | { mode: "asistida" }
  | { mode: "reportes" };

const TYPE_OPTIONS: { value: FacturaType; label: string }[] = [
  { value: "Emitida", label: "Emitida" },
  { value: "Recibida", label: "Recibida" },
];

// ══════════════════════ Page shell ══════════════════════

export function FacturasPage() {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(PAGE_SIZE);
  const [editor, setEditor] = useState<EditorState>({ mode: "closed" });

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPageNumber(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search]);

  const q = useQuery({
    queryKey: ["administration", "facturas", { search: debouncedSearch, pageNumber, pageSize }],
    queryFn: () => searchFacturas({ search: debouncedSearch || undefined, pageNumber, pageSize, sortDir: "desc" }),
    placeholderData: keepPreviousData,
  });

  // Catálogos para resolver nombres (cliente/proveedor según tipo, y empresa).
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
  const projectsQ = useQuery({
    queryKey: ["projects", "options"],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });
  const nameOf = (items: { id: string; name: string }[] | undefined, id: string | null): string =>
    (id && items?.find((i) => i.id === id)?.name) || "—";
  // Emitida → cliente; Recibida → proveedor.
  const counterparty = (it: FacturaRow): string =>
    it.type === "Emitida" ? nameOf(clientsQ.data?.items, it.clientId) : nameOf(suppliersQ.data?.items, it.supplierId);

  const data = q.data;
  const ctl = useTableControls(data?.items ?? [], {
    numero: (it) => it.number,
    tipo: (it) => it.type,
    contraparte: (it) => counterparty(it),
    empresa: (it) => nameOf(companiesQ.data?.items, it.companyId),
    proyecto: (it) => nameOf(projectsQ.data?.items, it.projectId),
    fecha: (it) => it.invoiceDate,
    vencimiento: (it) => it.dueDate,
    total: (it) => it.total,
    pendiente: (it) => it.total - it.collected,
    cobro: (it) => (it.total > 0 ? it.collected / it.total : 0),
  });
  const items = ctl.rows;
  const searchActive = debouncedSearch.length > 0;
  // Con filtros de columna activos la tabla sigue montada aunque no haya filas,
  // para que la cabecera (y sus filtros) siga accesible y se puedan cambiar/limpiar.
  const showEmpty = items.length === 0 && !ctl.hasActiveFilters;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={FileText}
        title="Facturas"
        total={data?.totalCount ?? null}
        unit="factura"
        description="Facturas emitidas y recibidas, con sus líneas de flujo de caja."
      >
        <Button
          variant="outline"
          onClick={() => setEditor({ mode: "reportes" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <FileBarChart2 className="size-4" />
          Reportes
        </Button>
        <Button
          variant="outline"
          onClick={() => setEditor({ mode: "asistida" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Sparkles className="size-4" />
          Asistida
        </Button>
        <Button
          onClick={() => setEditor({ mode: "create" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Plus className="size-4" />
          Nueva
        </Button>
      </EntityPageHeader>

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por número, cliente, proveedor, empresa, banco o importe…" />

      {q.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={cols} />
      ) : showEmpty ? (
        <EntityEmpty
          icon={searchActive ? Search : FileText}
          title={searchActive ? "Sin resultados" : "Aún no hay facturas"}
          body={searchActive ? `Nada coincide con "${debouncedSearch}".` : "Crea la primera factura para empezar."}
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
                aria-label={`Editar factura ${it.number}`}
              >
                <div className="min-w-0">
                  <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.number || "—"}</p>
                  <p className="truncate text-[12px] text-[var(--color-muted-foreground)]">
                    {`${it.type} · ${counterparty(it)} · ${it.invoiceDate ? it.invoiceDate.slice(0, 10) : "—"} · ${fmtMoney(it.total)}`}
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
                  ["proyecto", "Proyecto", undefined],
                  ["fecha", "Fecha", undefined],
                  ["vencimiento", "Vencim.", undefined],
                  ["total", "Total", "right"],
                  ["pendiente", "Pendiente", "right"],
                  ["cobro", "Cobro", undefined],
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
            {items.map((it, i) => (
              <EntityListRow key={it.id} className={cols} isLast={i === items.length - 1}>
                <span className="flex min-w-0 items-center gap-2">
                  <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.number || "—"}</span>
                  {it.verified && (
                    <ShieldCheck className="size-3.5 shrink-0 text-[var(--color-primary)]" aria-label="Verificada" />
                  )}
                </span>
                <span>
                  <EntityStatusBadge tone={it.type === "Emitida" ? "info" : "default"}>{it.type}</EntityStatusBadge>
                </span>
                <span className="truncate text-[13px] text-[var(--color-muted-foreground)]" title={counterparty(it)}>
                  {counterparty(it)}
                </span>
                <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">
                  {nameOf(companiesQ.data?.items, it.companyId)}
                </span>
                <span
                  className="truncate text-[13px] text-[var(--color-muted-foreground)]"
                  title={nameOf(projectsQ.data?.items, it.projectId)}
                >
                  {nameOf(projectsQ.data?.items, it.projectId)}
                </span>
                <span className="text-[13px] tabular-nums text-[var(--color-muted-foreground)]">
                  {it.invoiceDate ? it.invoiceDate.slice(0, 10) : "—"}
                </span>
                <span className="text-[13px] tabular-nums text-[var(--color-muted-foreground)]">
                  {it.dueDate ? it.dueDate.slice(0, 10) : "—"}
                </span>
                <span className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmtMoney(it.total)}</span>
                <span className="text-right text-[13px] tabular-nums text-[var(--color-muted-foreground)]">
                  {fmtMoney(Math.max(0, it.total - it.collected))}
                </span>
                <CobroBar total={it.total} collected={it.collected} />
                <div className="flex items-center justify-end gap-1">
                  <button
                    type="button"
                    aria-label={`Vincular líneas de la factura ${it.number}`}
                    onClick={() => setEditor({ mode: "lines", item: it })}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)] group-hover:opacity-100"
                  >
                    <Link2 className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Editar factura ${it.number}`}
                    onClick={() => setEditor({ mode: "edit", item: it })}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"
                  >
                    <Pencil className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Vencimientos de la factura ${it.number}`}
                    onClick={() => setEditor({ mode: "vencimientos", item: it })}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)] group-hover:opacity-100"
                  >
                    <CalendarClock className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Borrar factura ${it.number}`}
                    onClick={() => setEditor({ mode: "delete", item: it })}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)] group-hover:opacity-100"
                  >
                    <Trash2 className="size-3.5" />
                  </button>
                </div>
              </EntityListRow>
            ))}
          </EntityListCard>

          <EntityPager
            page={data?.pageNumber ?? 1}
            totalPages={data?.totalPages ?? 1}
            hasPrev={!!data?.hasPrevious}
            hasNext={!!data?.hasNext}
            onPrev={() => setPageNumber((p) => Math.max(1, p - 1))}
            onNext={() => setPageNumber((p) => p + 1)}
            pageSize={pageSize}
            onPageSizeChange={(s) => {
              setPageSize(s);
              setPageNumber(1);
            }}
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

      <FacturaEditor
        state={editor}
        onClose={() => setEditor({ mode: "closed" })}
        onVencimientos={(it) => setEditor({ mode: "vencimientos", item: it })}
      />
      <DeleteDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <LineasDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <VencimientosDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <AsistidaDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <ReportesDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

// ══════════════════════ Reportes ══════════════════════

const SCOPES: { value: InvoiceReportScope; label: string }[] = [
  { value: "Todas", label: "Todas" },
  { value: "Vencidas", label: "Vencidas" },
  { value: "PorVencer", label: "Por vencer" },
  { value: "PorCobrar", label: "Pendientes" },
  { value: "Cobradas", label: "Cobradas / pagadas" },
];

function statusLabel(r: InvoiceReportRow): string {
  switch (r.status) {
    case "Settled":
      return r.type === "Emitida" ? "Cobrada" : "Pagada";
    case "Overdue":
      return "Vencida";
    case "Upcoming":
      return "Por vencer";
    default:
      return "Sin vencim.";
  }
}

function statusTone(r: InvoiceReportRow): "success" | "danger" | "info" | "default" {
  switch (r.status) {
    case "Settled":
      return "success";
    case "Overdue":
      return "danger";
    case "Upcoming":
      return "info";
    default:
      return "default";
  }
}

const csvEscape = (v: string) => (/[";\n]/.test(v) ? `"${v.replaceAll('"', '""')}"` : v);

function ReportesDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "reportes";

  const [scope, setScope] = useState<InvoiceReportScope>("Vencidas");
  const [type, setType] = useState<FacturaType | null>(null);
  const [companyId, setCompanyId] = useState<string | null>(null);
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [dueWithinDays, setDueWithinDays] = useState("");

  const companiesQ = useQuery({
    queryKey: ["administration", "companies", "options"],
    queryFn: () => companiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen,
  });

  const horizon = dueWithinDays.trim() ? Number.parseInt(dueWithinDays, 10) : undefined;
  const params = {
    scope,
    type: type ?? undefined,
    companyId: companyId ?? undefined,
    from: from || undefined,
    to: to || undefined,
    dueWithinDays: scope === "PorVencer" && horizon && horizon > 0 ? horizon : undefined,
  };

  const q = useQuery({
    queryKey: ["administration", "facturas", "report", params],
    queryFn: () => getInvoicesReport(params),
    enabled: isOpen,
    placeholderData: keepPreviousData,
  });

  const report = q.data;
  const rows = report?.rows ?? [];
  const scopeLabel = SCOPES.find((s) => s.value === scope)?.label ?? scope;

  const exportCsv = () => {
    const header = ["Número", "Tipo", "Cliente/Proveedor", "Empresa", "Fecha", "Vencimiento", "Total", "Cobrado/Pagado", "Pendiente", "Días vencida", "Situación"];
    const lines = rows.map((r) =>
      [
        r.number,
        r.type,
        r.counterpartyName ?? "",
        r.companyName ?? "",
        r.invoiceDate ? r.invoiceDate.slice(0, 10) : "",
        r.dueDate ? r.dueDate.slice(0, 10) : "",
        String(r.total).replace(".", ","),
        String(r.collected).replace(".", ","),
        String(r.pending).replace(".", ","),
        r.daysOverdue != null && r.daysOverdue > 0 ? String(r.daysOverdue) : "",
        statusLabel(r),
      ]
        .map(csvEscape)
        .join(";"),
    );
    // BOM para que Excel abra el UTF-8 con acentos bien.
    const blob = new Blob(["﻿" + [header.join(";"), ...lines].join("\r\n")], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `facturas-${scope.toLowerCase()}-${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const printReport = () => {
    const w = window.open("", "_blank", "noopener");
    if (!w) return;
    const rowsHtml = rows
      .map(
        (r) => `<tr>
          <td>${r.number}</td><td>${r.type}</td><td>${r.counterpartyName ?? "—"}</td><td>${r.companyName ?? "—"}</td>
          <td>${r.invoiceDate ? r.invoiceDate.slice(0, 10) : "—"}</td><td>${r.dueDate ? r.dueDate.slice(0, 10) : "—"}</td>
          <td class="num">${fmtMoney(r.total)}</td><td class="num">${fmtMoney(r.collected)}</td><td class="num">${fmtMoney(r.pending)}</td>
          <td>${statusLabel(r)}</td></tr>`,
      )
      .join("");
    w.document.write(`<!doctype html><html lang="es"><head><meta charset="utf-8"><title>Reporte de facturas — ${scopeLabel}</title>
      <style>
        body{font-family:system-ui,sans-serif;font-size:12px;margin:24px;color:#111}
        h1{font-size:18px;margin:0 0 2px} p{margin:0 0 14px;color:#555}
        table{width:100%;border-collapse:collapse} th,td{border-bottom:1px solid #ddd;padding:4px 6px;text-align:left}
        th{font-size:10px;text-transform:uppercase;letter-spacing:.04em;color:#555} .num{text-align:right;font-variant-numeric:tabular-nums}
      </style></head><body>
      <h1>Reporte de facturas — ${scopeLabel}</h1>
      <p>${rows.length} factura(s) · generado el ${new Date().toLocaleString("es-ES")}</p>
      <table><thead><tr><th>Número</th><th>Tipo</th><th>Cliente/Proveedor</th><th>Empresa</th><th>Fecha</th><th>Vencim.</th><th class="num">Total</th><th class="num">Cobrado</th><th class="num">Pendiente</th><th>Situación</th></tr></thead>
      <tbody>${rowsHtml}</tbody></table></body></html>`);
    w.document.close();
    w.focus();
    w.print();
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md sm:!max-w-4xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <FileBarChart2 className="size-4 text-[var(--color-primary)]" />
            Reportes de facturas
          </DialogTitle>
          <DialogDescription>
            Facturas vencidas, por vencer, cobradas o pendientes, calculadas contra sus líneas validadas de flujo de caja.
          </DialogDescription>
        </DialogHeader>
        <DialogBody className="max-h-[65vh] space-y-4 overflow-y-auto">
          {/* Alcance */}
          <div className="flex flex-wrap gap-1.5">
            {SCOPES.map((s) => (
              <button
                key={s.value}
                type="button"
                onClick={() => setScope(s.value)}
                className={`cursor-pointer rounded-full border px-3 py-1 text-[12.5px] font-medium transition-colors ${
                  scope === s.value
                    ? "border-[var(--color-primary)] bg-[var(--color-primary)] text-[var(--color-primary-foreground)]"
                    : "border-[var(--color-border)] text-[var(--color-muted-foreground)] hover:border-[var(--color-primary)] hover:text-[var(--color-foreground)]"
                }`}
              >
                {s.label}
              </button>
            ))}
          </div>

          {/* Filtros */}
          <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
            <Field id="rep-type" label="Tipo">
              <Combobox
                id="rep-type"
                label="Tipo"
                variant="field"
                value={type}
                onChange={(v) => setType((v as FacturaType) ?? null)}
                options={TYPE_OPTIONS.map((t) => ({ value: t.value, label: t.label }))}
                clearable
                placeholder="Todas"
                emptyOptionLabel="Todas"
              />
            </Field>
            <Field id="rep-company" label="Empresa">
              <Combobox
                id="rep-company"
                label="Empresa"
                variant="field"
                value={companyId}
                onChange={setCompanyId}
                options={toOptions(companiesQ.data?.items)}
                searchable
                clearable
                placeholder={companiesQ.isLoading ? "Cargando…" : "Todas"}
                emptyOptionLabel="Todas"
              />
            </Field>
            <Field id="rep-from" label="Fecha desde">
              <Input id="rep-from" type="date" value={from} onChange={(e) => setFrom(e.target.value)} />
            </Field>
            <Field id="rep-to" label="Fecha hasta">
              <Input id="rep-to" type="date" value={to} onChange={(e) => setTo(e.target.value)} />
            </Field>
          </div>
          {scope === "PorVencer" && (
            <Field id="rep-horizon" label="Vencen en los próximos (días)">
              <Input
                id="rep-horizon"
                type="number"
                min={1}
                value={dueWithinDays}
                onChange={(e) => setDueWithinDays(e.target.value)}
                placeholder="Sin límite"
                className="max-w-[200px]"
              />
            </Field>
          )}

          {/* Resumen (sobre todo lo filtrado, independiente del alcance) */}
          {report && (
            <div className="grid grid-cols-2 gap-2 sm:grid-cols-5">
              {[
                { label: "Facturas", value: String(report.totalCount), amount: report.totalAmount },
                { label: "Cobradas / pagadas", value: String(report.settled.count), amount: report.settled.amount },
                { label: "Pendientes", value: String(report.pending.count), amount: report.pending.amount },
                { label: "Vencidas", value: String(report.overdue.count), amount: report.overdue.amount, danger: report.overdue.count > 0 },
                { label: "Por vencer", value: String(report.upcoming.count), amount: report.upcoming.amount },
              ].map((c) => (
                <div key={c.label} className="rounded-lg border border-[var(--color-border)] px-3 py-2">
                  <p className="truncate text-[11px] uppercase tracking-wide text-[var(--color-muted-foreground)]">{c.label}</p>
                  <p className={`text-[14px] font-semibold tabular-nums ${c.danger ? "text-[var(--color-destructive)]" : "text-[var(--color-foreground)]"}`}>
                    {c.value}
                  </p>
                  <p className="text-[11px] tabular-nums text-[var(--color-muted-foreground)]">{fmtMoney(c.amount)} €</p>
                </div>
              ))}
            </div>
          )}

          {/* Resultado */}
          {q.isLoading ? (
            <p className="text-[13px] text-[var(--color-muted-foreground)]">Generando reporte…</p>
          ) : q.isError ? (
            <p role="alert" className="text-[13px] text-[var(--color-destructive)]">{describe(q.error)}</p>
          ) : rows.length === 0 ? (
            <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-6 text-center text-[13px] text-[var(--color-muted-foreground)]">
              Ninguna factura en «{scopeLabel}» con los filtros actuales.
            </p>
          ) : (
            <div className="overflow-x-auto rounded-lg border border-[var(--color-border)]">
              <table className="w-full min-w-[720px] text-[12.5px]">
                <thead>
                  <tr className="border-b border-[var(--color-border)] text-left text-[10.5px] uppercase tracking-wide text-[var(--color-muted-foreground)]">
                    <th className="px-2.5 py-2 font-semibold">Número</th>
                    <th className="px-2.5 py-2 font-semibold">Cliente / Proveedor</th>
                    <th className="px-2.5 py-2 font-semibold">Vencim.</th>
                    <th className="px-2.5 py-2 text-right font-semibold">Total</th>
                    <th className="px-2.5 py-2 text-right font-semibold">Pendiente</th>
                    <th className="px-2.5 py-2 font-semibold">Situación</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-[var(--color-border)]">
                  {rows.map((r) => (
                    <tr key={r.id}>
                      <td className="px-2.5 py-1.5 font-medium text-[var(--color-foreground)]">
                        {r.number}
                        <span className="ml-1.5 text-[11px] font-normal text-[var(--color-muted-foreground)]">{r.type}</span>
                      </td>
                      <td className="max-w-[220px] truncate px-2.5 py-1.5 text-[var(--color-muted-foreground)]" title={r.counterpartyName ?? undefined}>
                        {r.counterpartyName ?? "—"}
                      </td>
                      <td className="px-2.5 py-1.5 tabular-nums text-[var(--color-muted-foreground)]">
                        {r.dueDate ? r.dueDate.slice(0, 10) : "—"}
                        {r.status === "Overdue" && r.daysOverdue != null && (
                          <span className="ml-1 text-[11px] text-[var(--color-destructive)]">+{r.daysOverdue}d</span>
                        )}
                      </td>
                      <td className="px-2.5 py-1.5 text-right tabular-nums text-[var(--color-foreground)]">{fmtMoney(r.total)}</td>
                      <td className="px-2.5 py-1.5 text-right tabular-nums text-[var(--color-muted-foreground)]">
                        {r.status === "Settled" ? "—" : fmtMoney(r.pending)}
                      </td>
                      <td className="px-2.5 py-1.5">
                        <EntityStatusBadge tone={statusTone(r)}>{statusLabel(r)}</EntityStatusBadge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </DialogBody>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline">Cerrar</Button>
          </DialogClose>
          <Button type="button" variant="outline" onClick={printReport} disabled={rows.length === 0}>
            <Printer className="mr-1.5 size-4" />
            Imprimir
          </Button>
          <Button type="button" onClick={exportCsv} disabled={rows.length === 0}>
            <Download className="mr-1.5 size-4" />
            Exportar CSV
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ══════════════════════ Vinculación asistida ══════════════════════

function AsistidaDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "asistida";
  const queryClient = useQueryClient();
  // Deselected suggestions (default = everything selected); keyed by lineId, reset on open.
  const [excluded, setExcluded] = useState<Set<string>>(new Set());
  useEffect(() => {
    if (isOpen) setExcluded(new Set());
  }, [isOpen]);

  const q = useQuery({
    queryKey: ["administration", "facturas", "link-suggestions"],
    queryFn: () => getLinkSuggestions(),
    enabled: isOpen,
    staleTime: 0,
  });

  const apply = useMutation({
    mutationFn: (pairs: LinkPair[]) => applyLinkSuggestions(pairs),
    onSuccess: (res) => {
      toast.success(`Vinculadas ${res.linked} líneas`, {
        description: res.skipped > 0 ? `${res.skipped} omitidas (ya vinculadas o desfasadas).` : undefined,
      });
      queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      queryClient.invalidateQueries({ queryKey: ["cashflow"] });
    },
    onError: (err) => toast.error("Error al vincular", { description: describe(err) }),
  });

  const data = q.data;
  const suggestions = data?.suggestions ?? [];
  const selectedCount = suggestions.length - excluded.size;

  const toggle = (lineId: string) =>
    setExcluded((prev) => {
      const next = new Set(prev);
      if (next.has(lineId)) next.delete(lineId);
      else next.add(lineId);
      return next;
    });

  const onApply = () => {
    const pairs: LinkPair[] = suggestions
      .filter((s) => !excluded.has(s.lineId))
      .map((s) => ({ invoiceId: s.invoiceId, lineId: s.lineId, kind: s.kind }));
    if (pairs.length > 0) apply.mutate(pairs);
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Sparkles className="size-4 text-[var(--color-primary)]" />
            Vinculación asistida
          </DialogTitle>
          <DialogDescription>
            Propuestas inequívocas factura ↔ línea por contraparte e importe (exacto o hito de la forma
            de pago). Lo ambiguo no se propone: se resuelve a mano desde 🔗.
          </DialogDescription>
        </DialogHeader>
        <DialogBody className="space-y-4">
          {data && (
            <div className="grid grid-cols-3 gap-2">
              {[
                { label: "Sugerencias", value: suggestions.length },
                { label: "Facturas sin líneas", value: data.invoicesWithoutLines },
                { label: "Ambiguas (manual)", value: data.ambiguousInvoices },
              ].map((c) => (
                <div key={c.label} className="rounded-lg border border-[var(--color-border)] px-3 py-2">
                  <p className="text-[11px] uppercase tracking-wide text-[var(--color-muted-foreground)]">{c.label}</p>
                  <p className="text-[14px] font-semibold tabular-nums text-[var(--color-foreground)]">{c.value}</p>
                </div>
              ))}
            </div>
          )}

          {q.isLoading ? (
            <p className="text-[13px] text-[var(--color-muted-foreground)]">Calculando sugerencias…</p>
          ) : q.isError ? (
            <p role="alert" className="text-[13px] text-[var(--color-destructive)]">{describe(q.error)}</p>
          ) : suggestions.length === 0 ? (
            <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-3 text-[13px] text-[var(--color-muted-foreground)]">
              No hay sugerencias inequívocas pendientes. El resto de vínculos se hace manualmente factura a factura.
            </p>
          ) : (
            <ul className="max-h-[45vh] divide-y divide-[var(--color-border)] overflow-y-auto rounded-lg border border-[var(--color-border)]">
              {suggestions.map((s) => {
                const checked = !excluded.has(s.lineId);
                return (
                  <li key={s.lineId} className="flex items-center gap-3 px-3 py-2">
                    <input
                      type="checkbox"
                      checked={checked}
                      onChange={() => toggle(s.lineId)}
                      aria-label={`Incluir sugerencia para ${s.invoiceNumber}`}
                      className="size-4 shrink-0 cursor-pointer accent-[var(--color-primary)]"
                    />
                    <div className="min-w-0 flex-1">
                      <p className="truncate text-[13px] font-medium text-[var(--color-foreground)]">
                        {s.invoiceNumber}
                        <span className="ml-1.5 font-normal text-[var(--color-muted-foreground)]">
                          {s.counterpartyName ?? "—"} · {fmtMoney(s.invoiceTotal)}
                        </span>
                      </p>
                      <p className="truncate text-[12px] text-[var(--color-muted-foreground)]">
                        → {fmtMoney(s.lineAmount)}
                        {s.lineDate ? ` · ${s.lineDate.slice(0, 10)}` : ""}
                        {s.projectName ? ` · ${s.projectName}` : s.lineDescription ? ` · ${s.lineDescription}` : ""}
                      </p>
                    </div>
                    <EntityStatusBadge tone={s.reason.startsWith("HITO") ? "info" : "success"}>
                      {s.reason}
                    </EntityStatusBadge>
                  </li>
                );
              })}
            </ul>
          )}
        </DialogBody>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={apply.isPending}>
              Cerrar
            </Button>
          </DialogClose>
          <Button
            type="button"
            onClick={onApply}
            disabled={apply.isPending || selectedCount === 0}
          >
            {apply.isPending ? "Vinculando…" : `Vincular seleccionadas (${selectedCount})`}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// ══════════════════════ Vencimientos (solo lectura, en el editor) ══════════════════════

/** Detalle de vencimientos y su estado de cobro/pago dentro del editor de factura. */
function VencimientosSection({ item, onManage }: { item: FacturaRow; onManage?: () => void }) {
  const linesQ = useQuery({
    queryKey: ["administration", "facturas", "lines", item.id],
    queryFn: () => getFacturaLines(item.id),
  });
  const lines = linesQ.data;
  const paidOf = (l: InvoiceLineDto): number => (l.validated ? l.amount : 0);

  return (
    <div className="space-y-2 rounded-lg border border-[var(--color-border)] p-3">
      <div className="flex items-center justify-between gap-2">
        <p className="text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">
          Vencimientos y su detalle de pagos
        </p>
        <div className="flex items-center gap-2.5">
          {lines && (
            <p className="text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">
              Pendiente: <span className="font-semibold text-[var(--color-foreground)]">{fmtMoney(lines.pending)}</span>
            </p>
          )}
          {onManage && (
            <Button type="button" variant="outline" onClick={onManage} className="h-7 gap-1 rounded-md px-2.5 text-[12px]">
              <CalendarClock className="size-3.5" />
              Gestionar
            </Button>
          )}
        </div>
      </div>
      {linesQ.isLoading ? (
        <p className="text-[13px] text-[var(--color-muted-foreground)]">Cargando…</p>
      ) : (lines?.lines.length ?? 0) === 0 ? (
        <p className="text-[12px] text-[var(--color-muted-foreground)]">
          Sin vencimientos vinculados. Añádelos desde «Gestionar», o vincula líneas de caja existentes con el botón 🔗
          de la lista.
        </p>
      ) : (
        <>
          <div className="grid grid-cols-[minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_90px] items-center gap-2 text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
            <span>Vence</span>
            <span className="text-right">Importe</span>
            <span className="text-right">Pagado</span>
            <span className="text-right">Pendiente</span>
            <span>Estado</span>
          </div>
          {lines!.lines.map((l) => (
            <div key={l.id} className="grid grid-cols-[minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_90px] items-center gap-2 border-t border-[oklch(from_var(--color-border)_l_c_h_/_0.5)] pt-2">
              <span className="text-[12.5px] tabular-nums text-[var(--color-foreground)]">{l.date ? l.date.slice(0, 10) : "—"}</span>
              <span className="text-right text-[12.5px] tabular-nums text-[var(--color-foreground)]">{fmtMoney(l.amount)}</span>
              <span className="text-right text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">{fmtMoney(paidOf(l))}</span>
              <span className="text-right text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">{fmtMoney(l.amount - paidOf(l))}</span>
              <span>
                <EntityStatusBadge tone={l.validated ? "success" : l.confirmed ? "info" : "default"}>
                  {l.validated ? "Pagada" : l.confirmed ? "Confirmada" : "Abierto"}
                </EntityStatusBadge>
              </span>
            </div>
          ))}
        </>
      )}
    </div>
  );
}

// ══════════════════════ Vencimientos (edición) dialog ══════════════════════

type VencimientoEdit = { date: string; percent: string; amount: string };

/** Edición de los vencimientos de una factura: fecha, % ↔ importe y marcar cobrado/pagado. */
function VencimientosDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "vencimientos";
  const item = state.mode === "vencimientos" ? state.item : undefined;
  const queryClient = useQueryClient();
  const paidWord = item?.type === "Emitida" ? "cobrado" : "pagado";

  const linesQ = useQuery({
    queryKey: ["administration", "facturas", "lines", item?.id],
    queryFn: () => getFacturaLines(item!.id),
    enabled: isOpen && !!item,
  });
  const lines = linesQ.data;

  // Borrador editable por línea; se resiembra cada vez que llegan datos frescos del servidor.
  const [edits, setEdits] = useState<Record<string, VencimientoEdit>>({});
  useEffect(() => {
    if (!isOpen || !lines) return;
    setEdits(
      Object.fromEntries(
        lines.lines.map((l) => [
          l.id,
          {
            date: l.date ? l.date.slice(0, 10) : "",
            percent: l.percentage != null ? String(l.percentage) : "",
            amount: String(l.amount),
          },
        ]),
      ),
    );
  }, [isOpen, lines]);

  // Filas nuevas aún sin crear (p. ej. dividir un pago en dos plazos).
  const [newRows, setNewRows] = useState<VencimientoEdit[]>([]);
  useEffect(() => {
    if (isOpen) setNewRows([]);
  }, [isOpen]);

  const fmt2 = (n: number): string => String(Math.round(n * 100) / 100);
  const total = item?.total ?? 0;
  const setEdit = (id: string, patch: Partial<VencimientoEdit>) =>
    setEdits((es) => ({ ...es, [id]: { ...es[id], ...patch } }));
  const onPercent = (id: string, v: string) => {
    const pct = toNumN(v);
    setEdit(id, { percent: v, ...(pct !== null && total > 0 ? { amount: fmt2((total * pct) / 100) } : {}) });
  };
  const onAmount = (id: string, v: string) => {
    const amount = toNumN(v);
    setEdit(id, { amount: v, ...(amount !== null && total > 0 ? { percent: fmt2((amount / total) * 100) } : {}) });
  };
  const setNewRow = (i: number, patch: Partial<VencimientoEdit>) =>
    setNewRows((rs) => rs.map((r, j) => (j === i ? { ...r, ...patch } : r)));
  const onNewPercent = (i: number, v: string) => {
    const pct = toNumN(v);
    setNewRow(i, { percent: v, ...(pct !== null && total > 0 ? { amount: fmt2((total * pct) / 100) } : {}) });
  };
  const onNewAmount = (i: number, v: string) => {
    const amount = toNumN(v);
    setNewRow(i, { amount: v, ...(amount !== null && total > 0 ? { percent: fmt2((amount / total) * 100) } : {}) });
  };

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
    queryClient.invalidateQueries({ queryKey: ["facturacion"] });
    queryClient.invalidateQueries({ queryKey: ["projects"] });
    queryClient.invalidateQueries({ queryKey: ["cashflow"] });
  };

  const upd = useMutation({
    // Per-call data travels through mutate(vars), never closed-over state.
    mutationFn: (vars: { line: InvoiceLineDto; date: string; amount: number; percentage: number | null }) => {
      const base = {
        amount: vars.amount,
        date: dateToIso(vars.date),
        percentage: vars.percentage,
        description: vars.line.description,
        projectId: vars.line.projectId,
        statusId: vars.line.statusId,
      };
      return vars.line.kind === "Income"
        ? updateIncome(vars.line.id, base)
        : updatePayment(vars.line.id, { ...base, supplierId: vars.line.supplierId });
    },
    onSuccess: () => {
      toast.success("Vencimiento actualizado");
      invalidate();
    },
    onError: (err) => toast.error("Error al guardar el vencimiento", { description: describe(err) }),
  });

  const paid = useMutation({
    mutationFn: (vars: { line: InvoiceLineDto; value: boolean }) =>
      vars.line.kind === "Income" ? validateIncome(vars.line.id, vars.value) : validatePayment(vars.line.id, vars.value),
    onSuccess: (_d, vars) => {
      toast.success(vars.value ? `Marcado como ${paidWord}` : `Ya no está ${paidWord}`);
      invalidate();
    },
    onError: (err) => toast.error("Error al cambiar el estado", { description: describe(err) }),
  });

  const add = useMutation({
    // Per-call data travels through mutate(vars), never closed-over state.
    mutationFn: (vars: {
      index: number;
      invoice: FacturaRow;
      date: string;
      amount: number;
      percentage: number | null;
    }) => {
      const base = {
        amount: vars.amount,
        date: dateToIso(vars.date),
        percentage: vars.percentage,
        invoiceId: vars.invoice.id,
        projectId: vars.invoice.projectId,
        description: `Vencimiento ${vars.invoice.number}`,
      };
      return vars.invoice.type === "Emitida"
        ? createIncome(base)
        : createPayment({ ...base, supplierId: vars.invoice.supplierId });
    },
    onSuccess: (_id, vars) => {
      toast.success("Vencimiento añadido");
      setNewRows((rs) => rs.filter((_, j) => j !== vars.index));
      invalidate();
    },
    onError: (err) => toast.error("Error al añadir el vencimiento", { description: describe(err) }),
  });

  const origDate = (l: InvoiceLineDto): string => (l.date ? l.date.slice(0, 10) : "");
  const isDirty = (l: InvoiceLineDto, e: VencimientoEdit | undefined): boolean =>
    !!e && (e.date !== origDate(l) || toNumN(e.amount) !== l.amount || toNumN(e.percent) !== l.percentage);

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md sm:!max-w-2xl">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <CalendarClock className="size-4 text-[var(--color-primary)]" />
            Vencimientos de {item?.number}
          </DialogTitle>
          <DialogDescription>
            Ajusta fecha, porcentaje o importe de cada plazo y márcalo como {paidWord} cuando se liquide.
          </DialogDescription>
        </DialogHeader>
        <DialogBody className="space-y-4">
          <div className="grid grid-cols-3 gap-2">
            {[
              { label: "Total", value: lines?.total },
              { label: item?.type === "Emitida" ? "Cobrado" : "Pagado", value: lines?.validatedAmount },
              { label: "Pendiente", value: lines?.pending },
            ].map((c) => (
              <div key={c.label} className="rounded-lg border border-[var(--color-border)] px-3 py-2">
                <p className="text-[11px] uppercase tracking-wide text-[var(--color-muted-foreground)]">{c.label}</p>
                <p className="text-[14px] font-semibold tabular-nums text-[var(--color-foreground)]">
                  {c.value != null ? fmtMoney(c.value) : "—"}
                </p>
              </div>
            ))}
          </div>

          {linesQ.isLoading ? (
            <p className="text-[13px] text-[var(--color-muted-foreground)]">Cargando…</p>
          ) : (
            <div className="space-y-2">
              <div className="flex items-center justify-between gap-2">
                <p className="text-[12px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
                  Plazos ({lines?.lines.length ?? 0})
                </p>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => setNewRows((rs) => [...rs, { date: "", percent: "", amount: "" }])}
                  className="h-8 rounded-lg px-3 text-[12.5px]"
                >
                  <Plus className="mr-1 size-3.5" />
                  Añadir vencimiento
                </Button>
              </div>

              {(lines?.lines.length ?? 0) === 0 && newRows.length === 0 && (
                <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-3 text-[13px] text-[var(--color-muted-foreground)]">
                  Esta factura no tiene vencimientos. Añade uno nuevo, o vincula líneas de caja existentes con el botón
                  🔗 de la lista.
                </p>
              )}

              {((lines?.lines.length ?? 0) > 0 || newRows.length > 0) && (
                <div className="grid grid-cols-[minmax(0,1.1fr)_72px_minmax(0,0.9fr)_150px] items-center gap-2 text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
                  <span>Vence</span>
                  <span>%</span>
                  <span>Importe</span>
                  <span />
                </div>
              )}
              {(lines?.lines ?? []).map((l) => {
                const e = edits[l.id];
                const dirty = isDirty(l, e);
                const amountOk = e ? toNumN(e.amount) !== null : false;
                return (
                  <div
                    key={l.id}
                    className="grid grid-cols-[minmax(0,1.1fr)_72px_minmax(0,0.9fr)_150px] items-center gap-2 border-t border-[oklch(from_var(--color-border)_l_c_h_/_0.5)] pt-2"
                  >
                    <Input
                      type="date"
                      value={e?.date ?? ""}
                      onChange={(ev) => setEdit(l.id, { date: ev.target.value })}
                      aria-label="Fecha del vencimiento"
                    />
                    <Input
                      type="number"
                      step="0.01"
                      min={0}
                      max={100}
                      value={e?.percent ?? ""}
                      onChange={(ev) => onPercent(l.id, ev.target.value)}
                      placeholder="%"
                      aria-label="Porcentaje del vencimiento"
                    />
                    <Input
                      type="number"
                      step="0.01"
                      value={e?.amount ?? ""}
                      onChange={(ev) => onAmount(l.id, ev.target.value)}
                      placeholder="0.00"
                      aria-label="Importe del vencimiento"
                    />
                    <div className="flex items-center justify-end gap-1.5">
                      {dirty ? (
                        <Button
                          type="button"
                          disabled={upd.isPending || !amountOk}
                          onClick={() =>
                            e &&
                            upd.mutate({
                              line: l,
                              date: e.date,
                              amount: toNumN(e.amount) ?? 0,
                              percentage: toNumN(e.percent),
                            })
                          }
                          className="h-7 rounded-md px-2.5 text-[12px]"
                        >
                          Guardar
                        </Button>
                      ) : (
                        <EntityStatusBadge tone={l.validated ? "success" : l.confirmed ? "info" : "default"}>
                          {l.validated ? (item?.type === "Emitida" ? "Cobrada" : "Pagada") : l.confirmed ? "Confirmada" : "Prevista"}
                        </EntityStatusBadge>
                      )}
                      <button
                        type="button"
                        title={l.validated ? `Quitar la marca de ${paidWord}` : `Marcar como ${paidWord}`}
                        aria-label={l.validated ? `Quitar la marca de ${paidWord}` : `Marcar como ${paidWord}`}
                        disabled={paid.isPending}
                        onClick={() => paid.mutate({ line: l, value: !l.validated })}
                        className={`grid size-7 shrink-0 cursor-pointer place-items-center rounded-md transition-colors hover:bg-[var(--color-muted)] disabled:opacity-50 ${
                          l.validated
                            ? "text-[var(--color-success)] hover:text-[var(--color-muted-foreground)]"
                            : "text-[var(--color-muted-foreground)] hover:text-[var(--color-success)]"
                        }`}
                      >
                        {l.validated ? <Undo2 className="size-3.5" /> : <CheckCircle2 className="size-3.5" />}
                      </button>
                    </div>
                  </div>
                );
              })}
              {newRows.map((r, i) => {
                const amountOk = toNumN(r.amount) !== null;
                return (
                  <div
                    key={`new-${i}`}
                    className="grid grid-cols-[minmax(0,1.1fr)_72px_minmax(0,0.9fr)_150px] items-center gap-2 border-t border-[oklch(from_var(--color-border)_l_c_h_/_0.5)] pt-2"
                  >
                    <Input
                      type="date"
                      value={r.date}
                      onChange={(ev) => setNewRow(i, { date: ev.target.value })}
                      aria-label={`Fecha del nuevo vencimiento ${i + 1}`}
                    />
                    <Input
                      type="number"
                      step="0.01"
                      min={0}
                      max={100}
                      value={r.percent}
                      onChange={(ev) => onNewPercent(i, ev.target.value)}
                      placeholder="%"
                      aria-label={`Porcentaje del nuevo vencimiento ${i + 1}`}
                    />
                    <Input
                      type="number"
                      step="0.01"
                      value={r.amount}
                      onChange={(ev) => onNewAmount(i, ev.target.value)}
                      placeholder="0.00"
                      aria-label={`Importe del nuevo vencimiento ${i + 1}`}
                    />
                    <div className="flex items-center justify-end gap-1.5">
                      <Button
                        type="button"
                        disabled={add.isPending || !amountOk || !item}
                        onClick={() =>
                          item &&
                          add.mutate({
                            index: i,
                            invoice: item,
                            date: r.date,
                            amount: toNumN(r.amount) ?? 0,
                            percentage: toNumN(r.percent),
                          })
                        }
                        className="h-7 rounded-md px-2.5 text-[12px]"
                      >
                        Guardar
                      </Button>
                      <button
                        type="button"
                        aria-label={`Quitar el nuevo vencimiento ${i + 1}`}
                        onClick={() => setNewRows((rs) => rs.filter((_, j) => j !== i))}
                        className="grid size-7 shrink-0 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
                      >
                        <Trash2 className="size-3.5" />
                      </button>
                    </div>
                  </div>
                );
              })}
              {lines && lines.lines.length > 0 && Math.abs(lines.linkedAmount - lines.total) > 0.005 && (
                <p className="rounded-lg bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.07)] px-3 py-2 text-[12.5px] text-[var(--color-destructive)]">
                  Los plazos suman {fmtMoney(lines.linkedAmount)} y la factura {fmtMoney(lines.total)} — diferencia de{" "}
                  {fmtMoney(lines.total - lines.linkedAmount)}.
                </p>
              )}
              <p className="text-[11.5px] text-[var(--color-muted-foreground)]">
                El % y el importe se recalculan entre sí sobre el total de la factura ({fmtMoney(total)}). La barra de
                cobro de la lista avanza con los plazos marcados como {paidWord}s.
              </p>
            </div>
          )}
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

// ══════════════════════ Líneas (link/unlink) dialog ══════════════════════

function LineasDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "lines";
  const item = state.mode === "lines" ? state.item : undefined;
  const kind: "Income" | "Payment" = item?.type === "Emitida" ? "Income" : "Payment";
  const queryClient = useQueryClient();

  const [candSearch, setCandSearch] = useState("");
  const [debounced, setDebounced] = useState("");
  useEffect(() => {
    const t = setTimeout(() => setDebounced(candSearch.trim()), 250);
    return () => clearTimeout(t);
  }, [candSearch]);
  useEffect(() => {
    if (isOpen) {
      setCandSearch("");
      setDebounced("");
    }
  }, [isOpen]);

  const linesQ = useQuery({
    queryKey: ["administration", "facturas", "lines", item?.id],
    queryFn: () => getFacturaLines(item!.id),
    enabled: isOpen && !!item,
  });

  // Candidates: unlinked cash lines of the matching side (Income↔Emitida, Payment↔Recibida).
  const candidatesQ = useQuery({
    queryKey: ["administration", "facturas", "candidates", item?.id, kind, debounced],
    queryFn: () =>
      kind === "Income"
        ? searchIncomes({ unlinked: true, search: debounced || undefined, pageSize: 8, sortBy: "date", sortDir: "desc" })
        : searchPayments({ unlinked: true, search: debounced || undefined, pageSize: 8, sortBy: "date", sortDir: "desc" }),
    enabled: isOpen && !!item,
    placeholderData: keepPreviousData,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
    queryClient.invalidateQueries({ queryKey: ["projects"] });
    queryClient.invalidateQueries({ queryKey: ["cashflow"] });
  };

  const mutate = useMutation({
    // Per-call data travels through mutate(vars), never closed-over state.
    mutationFn: (vars: { lineId: string; invoiceId: string | null }) =>
      linkLine({ lineId: vars.lineId, kind, invoiceId: vars.invoiceId }),
    onSuccess: (_d, vars) => {
      toast.success(vars.invoiceId ? "Línea vinculada" : "Línea desvinculada");
      invalidate();
    },
    onError: (err) => toast.error("Error al vincular", { description: describe(err) }),
  });

  const lines = linesQ.data;
  const candidates = (candidatesQ.data?.items ?? []).filter((c) => !c.invoiceId);
  const lineLabel = kind === "Income" ? "ingreso" : "pago";

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-lg">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Link2 className="size-4 text-[var(--color-primary)]" />
            Líneas de {item?.number}
          </DialogTitle>
          <DialogDescription>
            {item?.type === "Emitida" ? "Ingresos" : "Pagos"} de caja vinculados a esta factura.
          </DialogDescription>
        </DialogHeader>
        <DialogBody className="space-y-5">
          {/* Reconciliation summary */}
          <div className="grid grid-cols-3 gap-2">
            {[
              { label: "Total", value: lines?.total },
              { label: "Vinculado", value: lines?.linkedAmount },
              { label: "Pendiente", value: lines?.pending },
            ].map((c) => (
              <div key={c.label} className="rounded-lg border border-[var(--color-border)] px-3 py-2">
                <p className="text-[11px] uppercase tracking-wide text-[var(--color-muted-foreground)]">{c.label}</p>
                <p className="text-[14px] font-semibold tabular-nums text-[var(--color-foreground)]">
                  {c.value != null ? fmtMoney(c.value) : "—"}
                </p>
              </div>
            ))}
          </div>

          {/* Linked lines */}
          <div>
            <p className="mb-1.5 text-[12px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
              Vinculadas ({lines?.lines.length ?? 0})
            </p>
            {linesQ.isLoading ? (
              <p className="text-[13px] text-[var(--color-muted-foreground)]">Cargando…</p>
            ) : (lines?.lines.length ?? 0) === 0 ? (
              <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-2 text-[13px] text-[var(--color-muted-foreground)]">
                Sin líneas vinculadas todavía.
              </p>
            ) : (
              <ul className="divide-y divide-[var(--color-border)] rounded-lg border border-[var(--color-border)]">
                {lines!.lines.map((l) => (
                  <li key={l.id} className="flex items-center gap-2 px-3 py-2">
                    <span className="w-24 shrink-0 text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">
                      {fmtMoney(l.amount)}
                    </span>
                    <span className="min-w-0 flex-1 truncate text-[12px] text-[var(--color-muted-foreground)]">
                      {l.date ? l.date.slice(0, 10) : "sin fecha"}
                      {l.percentage != null ? ` · ${l.percentage}%` : ""}
                      {l.validated ? " · validado" : l.confirmed ? " · confirmado" : " · previsto"}
                    </span>
                    <button
                      type="button"
                      aria-label="Desvincular línea"
                      disabled={mutate.isPending}
                      onClick={() => mutate.mutate({ lineId: l.id, invoiceId: null })}
                      className="grid size-7 shrink-0 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)] disabled:opacity-50"
                    >
                      <Unlink className="size-3.5" />
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Candidate picker */}
          <div>
            <p className="mb-1.5 text-[12px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
              Vincular {lineLabel} existente
            </p>
            <Input
              value={candSearch}
              onChange={(e) => setCandSearch(e.target.value)}
              placeholder={`Buscar ${lineLabel}s sin factura por descripción…`}
              aria-label={`Buscar ${lineLabel}s sin factura`}
            />
            <div className="mt-2">
              {candidatesQ.isLoading ? (
                <p className="text-[13px] text-[var(--color-muted-foreground)]">Cargando…</p>
              ) : candidates.length === 0 ? (
                <p className="rounded-lg border border-dashed border-[var(--color-border)] px-3 py-2 text-[13px] text-[var(--color-muted-foreground)]">
                  {debounced ? `Sin ${lineLabel}s libres que coincidan.` : `No quedan ${lineLabel}s sin factura.`}
                </p>
              ) : (
                <ul className="divide-y divide-[var(--color-border)] rounded-lg border border-[var(--color-border)]">
                  {candidates.map((c) => (
                    <li key={c.id} className="flex items-center gap-2 px-3 py-2">
                      <span className="w-24 shrink-0 text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">
                        {fmtMoney(c.amount)}
                      </span>
                      <span className="min-w-0 flex-1 truncate text-[12px] text-[var(--color-muted-foreground)]">
                        {c.date ? c.date.slice(0, 10) : "sin fecha"}
                        {c.description ? ` · ${c.description}` : ""}
                      </span>
                      <Button
                        type="button"
                        variant="outline"
                        disabled={mutate.isPending || !item}
                        onClick={() => item && mutate.mutate({ lineId: c.id, invoiceId: item.id })}
                        className="h-7 shrink-0 rounded-md px-2.5 text-[12px]"
                      >
                        <Link2 className="mr-1 size-3" />
                        Vincular
                      </Button>
                    </li>
                  ))}
                </ul>
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

// ══════════════════════ Delete dialog ══════════════════════

function DeleteDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const item = state.mode === "delete" ? state.item : undefined;
  const queryClient = useQueryClient();

  const del = useMutation({
    mutationFn: (id: string) => deleteFactura(id),
    onSuccess: () => {
      toast.success("Borrada");
      queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
      onClose();
    },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar factura</DialogTitle>
          <DialogDescription>
            Esto elimina permanentemente la factura{" "}
            <span className="font-medium text-[var(--color-foreground)]">{item?.number}</span>.
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

type FacturaForm = {
  type: FacturaType;
  number: string;
  dynamicsNumber: string;
  total: string;
  taxBase: string;
  vat: string;
  invoiceDate: string;
  dueDate: string;
  paymentTerms: string;
  bank: string;
  notes: string;
  clientId: string | null;
  supplierId: string | null;
  companyId: string | null;
  societyId: string | null;
  projectId: string | null;
  statusId: string | null;
  verified: boolean;
  documentPath: string | null;
};

const BLANK: FacturaForm = {
  type: "Emitida",
  number: "",
  dynamicsNumber: "",
  total: "",
  taxBase: "",
  vat: "",
  invoiceDate: "",
  dueDate: "",
  paymentTerms: "",
  bank: "",
  notes: "",
  clientId: null,
  supplierId: null,
  companyId: null,
  societyId: null,
  projectId: null,
  statusId: null,
  verified: false,
  documentPath: null,
};

function useLookupOptions(queryKey: string, loader: () => Promise<{ items: { id: string; name: string; code?: string | null }[] }>, enabled: boolean) {
  return useQuery({
    queryKey: ["administration", queryKey, "options"],
    queryFn: loader,
    enabled,
  });
}

function FacturaEditor({
  state,
  onClose,
  onVencimientos,
}: {
  state: EditorState;
  onClose: () => void;
  onVencimientos?: (item: FacturaRow) => void;
}) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [f, setF] = useState<FacturaForm>(BLANK);
  const queryClient = useQueryClient();

  useEffect(() => {
    if (isOpen) {
      setF(
        item
          ? {
              type: item.type,
              number: item.number,
              dynamicsNumber: item.dynamicsNumber ?? "",
              total: item.total != null ? String(item.total) : "",
              taxBase: item.taxBase != null ? String(item.taxBase) : "",
              vat: item.vat != null ? String(item.vat) : "",
              invoiceDate: isoToDate(item.invoiceDate),
              dueDate: isoToDate(item.dueDate),
              paymentTerms: item.paymentTerms ?? "",
              bank: item.bank ?? "",
              notes: item.notes ?? "",
              clientId: item.clientId,
              supplierId: item.supplierId,
              companyId: item.companyId,
              societyId: item.societyId,
              projectId: item.projectId,
              statusId: item.statusId,
              verified: item.verified,
              documentPath: item.documentPath,
            }
          : BLANK,
      );
      // Selector de tipo de IVA: inferir el más cercano a partir de los importes guardados.
      if (item && item.taxBase != null && item.taxBase !== 0 && item.vat != null) {
        const actual = (item.vat / item.taxBase) * 100;
        const closest = [21, 10, 4, 0].reduce((a, b) => (Math.abs(b - actual) < Math.abs(a - actual) ? b : a));
        setVatRate(closest);
      } else {
        setVatRate(21);
      }
    }
  }, [isOpen, item]);

  // ── Extracción IA desde documento (solo alta) ──
  const [aiState, setAiState] = useState<"idle" | "analyzing" | "done" | "error">("idle");
  const [aiNote, setAiNote] = useState<string | null>(null);
  const [dragOver, setDragOver] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  useEffect(() => {
    if (isOpen) {
      setAiState("idle");
      setAiNote(null);
      setDragOver(false);
    }
  }, [isOpen]);

  const extract = useMutation({
    mutationFn: (file: File) => extractInvoice(file),
    onMutate: () => setAiState("analyzing"),
    onSuccess: (d) => {
      // Fill the form with the AI proposal — the user reviews everything before saving.
      setF((s) => ({
        ...s,
        type: d.type ?? s.type,
        number: d.number ?? s.number,
        dynamicsNumber: d.dynamicsNumber ?? s.dynamicsNumber,
        total: d.total != null ? String(d.total) : s.total,
        taxBase: d.taxBase != null ? String(d.taxBase) : s.taxBase,
        vat: d.vat != null ? String(d.vat) : s.vat,
        invoiceDate: d.invoiceDate ? d.invoiceDate.slice(0, 10) : s.invoiceDate,
        dueDate: d.dueDate ? d.dueDate.slice(0, 10) : s.dueDate,
        paymentTerms: d.paymentTerms ?? s.paymentTerms,
        bank: d.bank ?? s.bank,
        notes: d.notes ?? s.notes,
        clientId: d.matchedClientId ?? s.clientId,
        supplierId: d.matchedSupplierId ?? s.supplierId,
        documentPath: d.documentPath,
      }));
      const unmatched =
        d.counterpartyName && !d.matchedClientId && !d.matchedSupplierId
          ? `Contraparte detectada: "${d.counterpartyName}" — no está en el catálogo, selecciónala o créala.`
          : null;
      setAiNote(unmatched);
      setAiState("done");
      toast.success("Datos propuestos por IA", { description: "Revisa y corrige antes de guardar." });
    },
    onError: (err) => {
      setAiState("error");
      toast.error("No se pudo leer el documento", { description: describe(err) });
    },
  });

  const onFilePicked = (file: File | undefined | null) => {
    if (!file || extract.isPending) return;
    extract.mutate(file);
  };

  const onDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setDragOver(false);
    onFilePicked(e.dataTransfer.files?.[0]);
  };

  const openDocument = useMutation({
    mutationFn: (id: string) => getInvoiceDocumentUrl(id),
    onSuccess: ({ url }) => window.open(url, "_blank", "noopener"),
    onError: (err) => toast.error("No se pudo abrir el documento", { description: describe(err) }),
  });

  // Adjuntar/reemplazar documento en una factura YA creada (digitalización posterior).
  const attachInputRef = useRef<HTMLInputElement>(null);
  const attach = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => attachInvoiceDocument(id, file),
    onSuccess: ({ documentPath }) => {
      setF((s) => ({ ...s, documentPath }));
      queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
      toast.success("Documento adjuntado a la factura");
    },
    onError: (err) => toast.error("No se pudo adjuntar el documento", { description: describe(err) }),
  });

  const clientsQ = useLookupOptions("clients", () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const suppliersQ = useLookupOptions("suppliers", () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const companiesQ = useLookupOptions("companies", () => companiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const societiesQ = useLookupOptions("societies", () => societiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const statusesQ = useLookupOptions("statuses", () => statusesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }), isOpen);
  const projectsQ = useQuery({
    queryKey: ["projects", "options"],
    queryFn: () => searchProjects({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen,
  });

  const save = useMutation({
    mutationFn: async ({
      input,
      verify,
      vencimientos,
    }: {
      input: FacturaInput;
      verify: boolean;
      vencimientos: { date: string; amount: number; percentage: number | null }[];
    }) => {
      if (item) {
        await updateFactura(item.id, input);
        if (verify) await verifyFactura(item.id);
        return item.id;
      }
      const id = await createFactura(input);
      // Fraccionamiento del cobro/pago: cada plazo nace como línea de caja prevista
      // (sin confirmar) vinculada a la factura — Emitida → ingresos; Recibida → pagos.
      for (const v of vencimientos) {
        const pct = v.percentage ?? (input.total > 0 ? Math.round((v.amount / input.total) * 10000) / 100 : null);
        const base = {
          amount: v.amount,
          date: dateToIso(v.date),
          percentage: pct,
          invoiceId: id,
          projectId: input.projectId,
          description: `Vencimiento ${input.number}`,
        };
        if (input.type === "Emitida") await createIncome(base);
        else await createPayment({ ...base, supplierId: input.supplierId });
      }
      return id;
    },
    onSuccess: (_id, vars) => {
      toast.success(
        item
          ? "Actualizada"
          : vars.vencimientos.length > 0
            ? `Creada con ${vars.vencimientos.length} vencimiento(s)`
            : "Creada",
      );
      queryClient.invalidateQueries({ queryKey: ["administration", "facturas"] });
      queryClient.invalidateQueries({ queryKey: ["facturacion"] });
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      queryClient.invalidateQueries({ queryKey: ["cashflow"] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const set = <K extends keyof FacturaForm>(k: K, v: FacturaForm[K]) => setF((s) => ({ ...s, [k]: v }));

  // Base/IVA/Total consistentes según el tipo de IVA elegido (21 % por defecto, España):
  //   · editar la base → IVA = base × tipo, total = base + IVA
  //   · editar el total → base = total ÷ (1 + tipo), IVA = total − base
  //   · editar el importe de IVA a mano → solo recalcula el total (base + IVA)
  // Campo vacío cuenta como 0. vatRate es solo de UI; en la BD se guarda el importe.
  const [vatRate, setVatRate] = useState(21);
  const num = (s: string): number => {
    const n = Number(s.replace(",", "."));
    return Number.isFinite(n) ? n : 0;
  };
  const fmt2 = (n: number): string => String(Math.round(n * 100) / 100);
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
  const onVatRateChange = (rate: number) => {
    setVatRate(rate);
    setF((s) => {
      if (s.taxBase.trim() === "" && s.total.trim() === "") return s;
      const vat = fmt2(num(s.taxBase) * (rate / 100));
      return { ...s, vat, total: fmt2(num(s.taxBase) + num(vat)) };
    });
  };

  // ── Fraccionamiento (solo al crear): plazos fecha+%+importe con plantillas ──
  // % ↔ importe se calculan mutuamente sobre el total de la factura:
  //   · editar el % → importe = total × % / 100
  //   · editar el importe → % = importe / total × 100
  type Plazo = { date: string; percent: string; amount: string };
  const [plazos, setPlazos] = useState<Plazo[]>([]);
  useEffect(() => {
    if (isOpen) setPlazos([]);
  }, [isOpen]);

  const addDays = (baseIso: string, days: number): string => {
    const d = baseIso ? new Date(baseIso) : new Date();
    d.setDate(d.getDate() + days);
    return d.toISOString().slice(0, 10);
  };
  const pctOf = (amount: number, total: number): string => (total > 0 ? fmt2((amount / total) * 100) : "");
  const applyTemplate = (tpl: string) => {
    const total = toNumN(f.total) ?? 0;
    const base = f.invoiceDate || new Date().toISOString().slice(0, 10);
    const make = (fractions: number[], offsets: number[]): Plazo[] => {
      const amounts = fractions.map((fr) => Math.round(total * fr * 100) / 100);
      const diff = Math.round((total - amounts.reduce((s, a) => s + a, 0)) * 100) / 100;
      amounts[amounts.length - 1] = Math.round((amounts[amounts.length - 1] + diff) * 100) / 100;
      return amounts.map((a, i) => ({ date: addDays(base, offsets[i]), percent: pctOf(a, total), amount: a.toFixed(2) }));
    };
    if (tpl === "contado") setPlazos(make([1], [0]));
    else if (tpl === "30") setPlazos(make([1], [30]));
    else if (tpl === "306090") setPlazos(make([1 / 3, 1 / 3, 1 / 3], [30, 60, 90]));
    else if (tpl === "40e") setPlazos(make([0.4, 0.3, 0.3], [0, 30, 60]));
  };
  const setPlazoDate = (i: number, v: string) =>
    setPlazos((ps) => ps.map((p, j) => (j === i ? { ...p, date: v } : p)));
  const setPlazoPercent = (i: number, v: string) =>
    setPlazos((ps) =>
      ps.map((p, j) => {
        if (j !== i) return p;
        const pct = toNumN(v);
        const total = toNumN(f.total) ?? 0;
        return { ...p, percent: v, amount: pct !== null && total > 0 ? fmt2((total * pct) / 100) : p.amount };
      }),
    );
  const setPlazoAmount = (i: number, v: string) =>
    setPlazos((ps) =>
      ps.map((p, j) => {
        if (j !== i) return p;
        const amount = toNumN(v);
        const total = toNumN(f.total) ?? 0;
        return { ...p, amount: v, percent: amount !== null ? pctOf(amount, total) : p.percent };
      }),
    );

  const trimmedNumber = f.number.trim();
  const totalNum = toNumN(f.total);
  const sumPlazos = plazos.reduce((s, p) => s + (toNumN(p.amount) ?? 0), 0);
  const plazosDiff = Math.round(((totalNum ?? 0) - sumPlazos) * 100) / 100;
  const plazosComplete = plazos.every((p) => p.date && toNumN(p.amount) !== null);
  const plazosOk = plazos.length === 0 || (plazosComplete && Math.abs(plazosDiff) < 0.005);
  const canSave = trimmedNumber.length > 0 && totalNum !== null && (!!item || plazosOk);

  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!canSave) return;
    const input: FacturaInput = {
      type: f.type,
      number: trimmedNumber,
      total: totalNum ?? 0,
      clientId: f.type === "Emitida" ? f.clientId : null,
      supplierId: f.type === "Recibida" ? f.supplierId : null,
      invoiceDate: dateToIso(f.invoiceDate),
      dueDate: dateToIso(f.dueDate),
      companyId: f.companyId,
      societyId: f.societyId,
      projectId: f.projectId,
      taxBase: toNumN(f.taxBase),
      vat: toNumN(f.vat),
      paymentTerms: nn(f.paymentTerms),
      bank: f.type === "Recibida" ? nn(f.bank) : null,
      statusId: f.statusId,
      dynamicsNumber: nn(f.dynamicsNumber),
      notes: nn(f.notes),
      documentPath: f.documentPath,
    };
    // Verify is a separate one-way action; only fire it on edit when newly toggled on.
    const verify = !!item && f.verified && !item.verified;
    save.mutate({
      input,
      verify,
      vencimientos: item ? [] : plazos.map((p) => ({ date: p.date, amount: toNumN(p.amount) ?? 0, percentage: toNumN(p.percent) })),
    });
  };

  const verifyLocked = !item || item.verified; // can't verify a not-yet-created invoice, nor un-verify one.

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md sm:!max-w-3xl">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{item ? "Editar factura" : "Nueva factura"}</DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">
            {!item && (
              <div>
                <div
                  role="button"
                  tabIndex={0}
                  aria-label="Subir documento de la factura para extraer los datos con IA"
                  onClick={() => !extract.isPending && fileInputRef.current?.click()}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") fileInputRef.current?.click();
                  }}
                  onDragOver={(e) => {
                    e.preventDefault();
                    setDragOver(true);
                  }}
                  onDragLeave={() => setDragOver(false)}
                  onDrop={onDrop}
                  className={`flex cursor-pointer items-center gap-3 rounded-lg border border-dashed px-3 py-3 transition-colors ${
                    dragOver
                      ? "border-[var(--color-primary)] bg-[oklch(from_var(--color-primary)_l_c_h_/_0.06)]"
                      : "border-[var(--color-border)] hover:border-[var(--color-primary)]"
                  }`}
                >
                  {aiState === "analyzing" ? (
                    <Loader2 className="size-5 shrink-0 animate-spin text-[var(--color-primary)]" />
                  ) : (
                    <ScanLine className="size-5 shrink-0 text-[var(--color-primary)]" />
                  )}
                  <div className="min-w-0">
                    <p className="text-[13px] font-medium text-[var(--color-foreground)]">
                      {aiState === "analyzing"
                        ? "Analizando documento con IA…"
                        : aiState === "done"
                          ? "Documento leído — propuesta aplicada"
                          : "Arrastra la factura (PDF o imagen) o haz clic para subirla"}
                    </p>
                    <p className="text-[11px] text-[var(--color-muted-foreground)]">
                      {aiState === "done"
                        ? "Revisa los campos antes de guardar. El documento quedará adjunto."
                        : "La IA propone los datos y tú los confirmas. Máx. 10 MB."}
                    </p>
                  </div>
                  {aiState === "done" && (
                    <Sparkles className="ml-auto size-4 shrink-0 text-[var(--color-primary)]" aria-label="Propuesta IA" />
                  )}
                </div>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="application/pdf,image/png,image/jpeg,image/webp,image/gif"
                  className="hidden"
                  onChange={(e) => {
                    onFilePicked(e.target.files?.[0]);
                    e.target.value = "";
                  }}
                />
                {aiNote && (
                  <p className="mt-1.5 text-[12px] text-[var(--color-muted-foreground)]">{aiNote}</p>
                )}
              </div>
            )}

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
                        {attach.isPending ? "Subiendo documento…" : "Adjuntar documento digitalizado"}
                      </span>
                      <span className="block text-[11px] text-[var(--color-muted-foreground)]">
                        Esta factura aún no tiene documento. PDF o imagen, máx. 10 MB.
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

            {/* 2 columnas en móvil, 3 en escritorio */}
            <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
              <Field id="fa-type" label="Tipo" required hint={item ? "El tipo no se puede cambiar." : undefined}>
                <Combobox
                  id="fa-type"
                  label="Tipo"
                  variant="field"
                  value={f.type}
                  onChange={(v) =>
                    setF((s) => ({ ...s, type: (v as FacturaType) ?? "Emitida", clientId: null, supplierId: null }))
                  }
                  options={TYPE_OPTIONS.map((t) => ({ value: t.value, label: t.label }))}
                  placeholder="Tipo"
                  disabled={!!item}
                />
              </Field>
              <Field id="fa-number" label="Número" required>
                <Input id="fa-number" value={f.number} onChange={(e) => set("number", e.target.value)} placeholder="Nº factura" autoFocus required maxLength={128} />
              </Field>
              <Field id="fa-dyn" label="Nº Dynamics">
                <Input id="fa-dyn" value={f.dynamicsNumber} onChange={(e) => set("dynamicsNumber", e.target.value)} placeholder="Opcional" maxLength={128} />
              </Field>

              {f.type === "Emitida" ? (
                <Field id="fa-client" label="Cliente">
                  <Combobox
                    id="fa-client"
                    label="Cliente"
                    variant="field"
                    value={f.clientId}
                    onChange={(v) => set("clientId", v)}
                    options={toOptions(clientsQ.data?.items)}
                    searchable
                    clearable
                    placeholder={clientsQ.isLoading ? "Cargando…" : "Sin asignar"}
                    emptyOptionLabel="Sin asignar"
                  />
                </Field>
              ) : (
                <Field id="fa-supplier" label="Proveedor">
                  <Combobox
                    id="fa-supplier"
                    label="Proveedor"
                    variant="field"
                    value={f.supplierId}
                    onChange={(v) => set("supplierId", v)}
                    options={toOptions(suppliersQ.data?.items)}
                    searchable
                    clearable
                    placeholder={suppliersQ.isLoading ? "Cargando…" : "Sin asignar"}
                    emptyOptionLabel="Sin asignar"
                  />
                </Field>
              )}
              <Field id="fa-company" label="Empresa">
                <Combobox
                  id="fa-company"
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
              <Field id="fa-society" label="Sociedad">
                <Combobox
                  id="fa-society"
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
              <Field id="fa-project" label="Proyecto" hint="Los vencimientos que crees aquí lo heredan.">
                <Combobox
                  id="fa-project"
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

              <Field id="fa-base" label="Base imponible">
                <Input id="fa-base" type="number" step="0.01" value={f.taxBase} onChange={(e) => setAmount("taxBase", e.target.value)} placeholder="0.00" />
              </Field>
              <Field id="fa-vat" label="IVA">
                <div className="flex gap-1.5">
                  <select
                    aria-label="Tipo de IVA"
                    value={vatRate}
                    onChange={(e) => onVatRateChange(Number(e.target.value))}
                    className="h-9 w-[74px] shrink-0 cursor-pointer rounded-lg border border-[var(--color-input)] bg-transparent px-2 text-[13px] text-[var(--color-foreground)] outline-none focus:border-[var(--color-primary)]"
                  >
                    <option value={21}>21 %</option>
                    <option value={10}>10 %</option>
                    <option value={4}>4 %</option>
                    <option value={0}>0 %</option>
                  </select>
                  <Input id="fa-vat" type="number" step="0.01" value={f.vat} onChange={(e) => setAmount("vat", e.target.value)} placeholder="0.00" />
                </div>
              </Field>
              <Field id="fa-total" label="Total" required>
                <Input id="fa-total" type="number" step="0.01" value={f.total} onChange={(e) => setAmount("total", e.target.value)} placeholder="0.00" required />
              </Field>

              <Field id="fa-date" label="Fecha factura">
                <Input id="fa-date" type="date" value={f.invoiceDate} onChange={(e) => set("invoiceDate", e.target.value)} />
              </Field>
              <Field id="fa-due" label="Vencimiento">
                <Input id="fa-due" type="date" value={f.dueDate} onChange={(e) => set("dueDate", e.target.value)} />
              </Field>
              <Field id="fa-status" label="Estado">
                <Combobox
                  id="fa-status"
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

              <Field id="fa-terms" label="Forma de pago">
                <Input id="fa-terms" value={f.paymentTerms} onChange={(e) => set("paymentTerms", e.target.value)} placeholder="Opcional" maxLength={256} />
              </Field>
              {f.type === "Recibida" && (
                <Field id="fa-bank" label="Banco">
                  <Input id="fa-bank" value={f.bank} onChange={(e) => set("bank", e.target.value)} placeholder="Opcional" maxLength={256} />
                </Field>
              )}
              <Field id="fa-notes" label="Notas" className={f.type === "Recibida" ? "" : "col-span-2"}>
                <Input id="fa-notes" value={f.notes} onChange={(e) => set("notes", e.target.value)} placeholder="Opcional" maxLength={1024} />
              </Field>
            </div>

            {!item && (
              <div className="space-y-3 rounded-lg border border-[var(--color-border)] p-3">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <p className="text-[11.5px] font-semibold uppercase tracking-wider text-[var(--color-muted-foreground)]">
                    Vencimientos
                  </p>
                  <div className="flex items-center gap-2">
                    <select
                      aria-label="Plantilla de vencimientos"
                      value=""
                      onChange={(e) => e.target.value && applyTemplate(e.target.value)}
                      className="h-8 cursor-pointer rounded-lg border border-[var(--color-input)] bg-[var(--color-card)] px-2 text-[12.5px] text-[var(--color-foreground)] outline-none focus:border-[var(--color-primary)]"
                    >
                      <option value="">Plantilla…</option>
                      <option value="contado">Contado</option>
                      <option value="30">30 días</option>
                      <option value="306090">30 / 60 / 90</option>
                      <option value="40e">40% entrada + 30 + 60</option>
                    </select>
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => setPlazos((ps) => [...ps, { date: "", percent: "", amount: "" }])}
                      className="h-8 rounded-lg px-3 text-[12.5px]"
                    >
                      Añadir línea
                    </Button>
                  </div>
                </div>

                {plazos.length === 0 ? (
                  <p className="text-[12px] text-[var(--color-muted-foreground)]">
                    Sin fraccionamiento: la factura se crea sin plazos (puedes vincular líneas después).
                  </p>
                ) : (
                  <>
                    <div className="grid grid-cols-[24px_minmax(0,1.2fr)_minmax(0,0.7fr)_minmax(0,1fr)_32px] items-center gap-2 text-[10.5px] font-semibold uppercase tracking-wide text-[var(--color-muted-foreground)]">
                      <span>#</span>
                      <span>Fecha</span>
                      <span>%</span>
                      <span>Importe</span>
                      <span />
                    </div>
                    {plazos.map((p, i) => (
                      <div key={i} className="grid grid-cols-[24px_minmax(0,1.2fr)_minmax(0,0.7fr)_minmax(0,1fr)_32px] items-center gap-2">
                        <span className="text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">{i + 1}</span>
                        <Input type="date" value={p.date} onChange={(e) => setPlazoDate(i, e.target.value)} aria-label={`Fecha del vencimiento ${i + 1}`} />
                        <Input
                          type="number"
                          step="0.01"
                          min={0}
                          max={100}
                          value={p.percent}
                          onChange={(e) => setPlazoPercent(i, e.target.value)}
                          placeholder="%"
                          aria-label={`Porcentaje del vencimiento ${i + 1}`}
                        />
                        <Input
                          type="number"
                          step="0.01"
                          value={p.amount}
                          onChange={(e) => setPlazoAmount(i, e.target.value)}
                          placeholder="0.00"
                          aria-label={`Importe del vencimiento ${i + 1}`}
                        />
                        <button
                          type="button"
                          aria-label={`Quitar vencimiento ${i + 1}`}
                          onClick={() => setPlazos((ps) => ps.filter((_, j) => j !== i))}
                          className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] transition-colors hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
                        >
                          <Trash2 className="size-3.5" />
                        </button>
                      </div>
                    ))}
                    {plazosOk ? (
                      <p className="rounded-lg bg-[oklch(from_var(--color-success)_l_c_h_/_0.08)] px-3 py-2 text-[12.5px] text-[var(--color-success)]">
                        Los vencimientos suman el total de la factura
                      </p>
                    ) : (
                      <p className="rounded-lg bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.07)] px-3 py-2 text-[12.5px] text-[var(--color-destructive)]">
                        {plazosComplete
                          ? `Diferencia de ${plazosDiff.toFixed(2)} € — ajusta antes de guardar`
                          : "Completa fecha e importe de todos los plazos"}
                      </p>
                    )}
                  </>
                )}
              </div>
            )}

            {item && (
              <VencimientosSection item={item} onManage={onVencimientos ? () => onVencimientos(item) : undefined} />
            )}

            <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2">
              <div className="min-w-0">
                <span className="text-[13px] text-[var(--color-foreground)]">Verificada</span>
                {verifyLocked && (
                  <p className="text-[11px] text-[var(--color-muted-foreground)]">
                    {item ? "Ya verificada." : "Disponible tras crear la factura."}
                  </p>
                )}
              </div>
              <Switch
                checked={f.verified}
                onCheckedChange={(v) => set("verified", v)}
                disabled={verifyLocked}
                aria-label="Verificada"
              />
            </div>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={save.isPending}>
                Cancelar
              </Button>
            </DialogClose>
            <Button type="submit" disabled={save.isPending || !canSave}>
              {save.isPending ? "Guardando…" : item ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
