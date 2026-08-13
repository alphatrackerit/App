import { useEffect, useRef, useState, type FormEvent, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Banknote, Building, Building2, Globe, Hash, Landmark, ListChecks, Pencil, Plus, Search, Trash2, Truck, Users, type LucideIcon } from "lucide-react";
import { toast } from "sonner";
import {
  bankCatalog,
  bankMovementsApi,
  clientCatalog,
  clientsApi,
  companyCatalog,
  countryCatalog,
  prefixesApi,
  societiesApi,
  statusCatalog,
  supplierCatalog,
  uploadCompanyLogo,
  type BankInput,
  type BankMovementRow,
  type BankRow,
  type ClientInput,
  type ClientRow,
  type CompanyInput,
  type CompanyRow,
  type CountryInput,
  type CountryRow,
  type Lookup,
  type PrefixInput,
  type PrefixRow,
  type PrefixType,
  type RichCatalogApi,
  type SocietyInput,
  type SocietyRow,
  type StatusInput,
  type StatusRow,
  type StatusType,
  type SupplierInput,
  type SupplierRow,
} from "@/api/administration";
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
  useTableState,
  useTableRows,
  type ColValue,
  Field,
  type ComboboxOption,
} from "@/components/list";
import { describe } from "@/lib/list-helpers";

const PAGE_SIZE = 20;

type Base = { id: string; name: string };

type EditorState<TRow> =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; item: TRow }
  | { mode: "delete"; item: TRow };

type EditorProps<TRow, TInput> = {
  state: EditorState<TRow>;
  api: RichCatalogApi<TRow, TInput>;
  queryKey: string;
  onClose: () => void;
};

// Small colored square for hex values (raw hex → not theme-aware, used as an accent only).
function ColorSwatch({ hex }: { hex: string | null }) {
  return (
    <span
      className="inline-block size-4 shrink-0 rounded-sm border border-[var(--color-border)]"
      style={{ background: hex ?? "transparent" }}
      aria-hidden
    />
  );
}

// ─── Generic shell: header + search + list + pager + delete dialog ───
type CatalogColumn<TRow> = { key: string; label: string; get: (row: TRow) => ColValue; align?: "right" };

function RichCatalogShell<TRow extends Base, TInput>({
  title,
  unit,
  icon,
  queryKey,
  api,
  description,
  cols,
  columns,
  rowCells,
  Editor,
}: {
  title: string;
  unit: string;
  icon: LucideIcon;
  queryKey: string;
  api: RichCatalogApi<TRow, TInput>;
  description?: string;
  cols: string;
  columns: CatalogColumn<TRow>[];
  rowCells: (item: TRow) => ReactNode;
  Editor: (props: EditorProps<TRow, TInput>) => ReactNode;
}) {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  // Sort/filtro por columna + paginación. Con un orden o filtro activo la
  // consulta se ensancha al dataset completo y la tabla pagina en memoria.
  const ctl = useTableState({ pageSize: PAGE_SIZE });
  const { setPage } = ctl;
  const [editor, setEditor] = useState<EditorState<TRow>>({ mode: "closed" });

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPage(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search, setPage]);

  const q = useQuery({
    queryKey: ["administration", queryKey, { search: debouncedSearch, ...ctl.fetch }],
    queryFn: () =>
      api.search({ search: debouncedSearch || undefined, ...ctl.fetch, sortBy: "name", sortDir: "asc" }),
    placeholderData: keepPreviousData,
  });

  const data = q.data;
  const view = useTableRows(
    data?.items ?? [],
    Object.fromEntries(columns.map((c) => [c.key, c.get])),
    ctl,
    data,
  );
  const items = view.rows;
  const searchActive = debouncedSearch.length > 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader icon={icon} title={title} total={view.totalCount} unit={unit} description={description}>
        <Button
          onClick={() => setEditor({ mode: "create" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Plus className="size-4" />
          Nuevo
        </Button>
      </EntityPageHeader>

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por nombre…" />

      {q.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={cols} />
      ) : items.length === 0 && !ctl.hasActiveFilters ? (
        <EntityEmpty
          icon={searchActive ? Search : icon}
          title={searchActive ? "Sin resultados" : "Aún no hay registros"}
          body={searchActive ? `Nada coincide con "${debouncedSearch}".` : "Crea el primer registro para empezar."}
          action={
            <Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]">
              <Plus className="mr-1.5 size-4" />
              Nuevo
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
                aria-label={`Editar ${it.name}`}
              >
                <div className="min-w-0">
                  <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</p>
                </div>
              </EntityMobileCard>
            ))}
          </div>

          <EntityListCard className="hidden md:block">
            <EntityListHeader className={cols}>
              {columns.map((c) => (
                <EntityColHeader
                  key={c.key}
                  colKey={c.key}
                  label={c.label}
                  align={c.align}
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
                {rowCells(it)}
                <div className="flex items-center justify-end gap-1">
                  <button
                    type="button"
                    aria-label={`Editar ${it.name}`}
                    onClick={() => setEditor({ mode: "edit", item: it })}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"
                  >
                    <Pencil className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Borrar ${it.name}`}
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

      <Editor state={editor} api={api} queryKey={queryKey} onClose={() => setEditor({ mode: "closed" })} />
      <DeleteDialog state={editor} api={api} queryKey={queryKey} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function DeleteDialog<TRow extends Base, TInput>({ state, api, queryKey, onClose }: EditorProps<TRow, TInput>) {
  const isOpen = state.mode === "delete";
  const item = state.mode === "delete" ? state.item : undefined;
  const queryClient = useQueryClient();

  const del = useMutation({
    mutationFn: (id: string) => api.remove(id),
    onSuccess: () => {
      toast.success("Borrado");
      queryClient.invalidateQueries({ queryKey: ["administration", queryKey] });
      onClose();
    },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar registro</DialogTitle>
          <DialogDescription>
            Esto elimina permanentemente <span className="font-medium text-[var(--color-foreground)]">{item?.name}</span>.
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

// Shared editor dialog frame (header + form + footer). Children are the fields.
function EditorFrame({
  isOpen,
  editing,
  title,
  saving,
  canSave,
  onClose,
  onSubmit,
  children,
}: {
  isOpen: boolean;
  editing: boolean;
  title: string;
  saving: boolean;
  canSave: boolean;
  onClose: () => void;
  onSubmit: (e: FormEvent<HTMLFormElement>) => void;
  children: ReactNode;
}) {
  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{editing ? `Editar ${title.toLowerCase()}` : `Nuevo en ${title}`}</DialogTitle>
          </DialogHeader>
          <DialogBody className="space-y-5">{children}</DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={saving}>
                Cancelar
              </Button>
            </DialogClose>
            <Button type="submit" disabled={saving || !canSave}>
              {saving ? "Guardando…" : editing ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function useSaveMutation<TRow extends Base, TInput>(
  api: RichCatalogApi<TRow, TInput>,
  queryKey: string,
  item: TRow | undefined,
  onClose: () => void,
) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: TInput) => (item ? api.update(item.id, input) : api.create(input)),
    onSuccess: () => {
      toast.success(item ? "Actualizado" : "Creado");
      queryClient.invalidateQueries({ queryKey: ["administration", queryKey] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });
}

function toOptions(items: Lookup[] | undefined): ComboboxOption[] {
  return (items ?? []).map((i) => ({ value: i.id, label: i.name, hint: i.code ?? undefined }));
}

// ─── shared form helpers ───
const nn = (s: string) => s.trim() || null; // "" → null for optional text fields
const isoToDate = (iso: string | null) => (iso ? iso.slice(0, 10) : ""); // ISO → yyyy-mm-dd
const dateToIso = (d: string) => (d ? `${d}T00:00:00Z` : null); // date input → ISO the backend parses
// hex fallback for <input type=color> when the stored value isn't #rrggbb (e.g. legacy rgba()).
const hexOr = (v: string, fallback: string) => (/^#[0-9a-fA-F]{6}$/.test(v) ? v : fallback);

function ColorPicker({ id, value, onChange, fallback }: { id: string; value: string; onChange: (v: string) => void; fallback: string }) {
  return (
    <div className="flex items-center gap-2">
      <input
        id={id}
        type="color"
        value={hexOr(value, fallback)}
        onChange={(e) => onChange(e.target.value)}
        className="h-9 w-12 cursor-pointer rounded-lg border border-[var(--color-input)] bg-transparent p-1"
      />
      <Input value={value} onChange={(e) => onChange(e.target.value)} placeholder={fallback} maxLength={32} />
    </div>
  );
}

// CRM fields shared by Client & Supplier. `type` is the client/supplier-type free text.
type PartyForm = {
  legalName: string;
  taxId: string;
  type: string;
  address: string;
  phone: string;
  email: string;
  contact: string;
  registeredOn: string;
  colorHex: string;
};

function PartyFieldset({
  p,
  f,
  set,
  typeLabel,
}: {
  p: string;
  f: PartyForm;
  set: (k: keyof PartyForm, v: string) => void;
  typeLabel: string;
}) {
  return (
    <>
      <Field id={`${p}-legal`} label="Razón social">
        <Input id={`${p}-legal`} value={f.legalName} onChange={(e) => set("legalName", e.target.value)} placeholder="Opcional" maxLength={256} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id={`${p}-tax`} label="NIF/CIF">
          <Input id={`${p}-tax`} value={f.taxId} onChange={(e) => set("taxId", e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
        <Field id={`${p}-type`} label={typeLabel}>
          <Input id={`${p}-type`} value={f.type} onChange={(e) => set("type", e.target.value)} placeholder="Opcional" maxLength={128} />
        </Field>
      </div>
      <Field id={`${p}-addr`} label="Dirección">
        <Input id={`${p}-addr`} value={f.address} onChange={(e) => set("address", e.target.value)} placeholder="Opcional" maxLength={512} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id={`${p}-phone`} label="Teléfono">
          <Input id={`${p}-phone`} value={f.phone} onChange={(e) => set("phone", e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
        <Field id={`${p}-email`} label="Email">
          <Input id={`${p}-email`} type="email" value={f.email} onChange={(e) => set("email", e.target.value)} placeholder="Opcional" maxLength={256} />
        </Field>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <Field id={`${p}-contact`} label="Contacto">
          <Input id={`${p}-contact`} value={f.contact} onChange={(e) => set("contact", e.target.value)} placeholder="Opcional" maxLength={256} />
        </Field>
        <Field id={`${p}-reg`} label="Fecha alta">
          <Input id={`${p}-reg`} type="date" value={f.registeredOn} onChange={(e) => set("registeredOn", e.target.value)} />
        </Field>
      </div>
    </>
  );
}

// ══════════════════════ Suppliers (color + priority) ══════════════════════

type SupplierForm = PartyForm & { name: string; code: string; priority: string };
const BLANK_SUPPLIER: SupplierForm = {
  name: "", code: "", legalName: "", taxId: "", type: "", address: "", phone: "", email: "", contact: "", registeredOn: "", colorHex: "#3b82f6", priority: "0",
};

function SupplierEditor({ state, api, queryKey, onClose }: EditorProps<SupplierRow, SupplierInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [f, setF] = useState<SupplierForm>(BLANK_SUPPLIER);

  useEffect(() => {
    if (isOpen) {
      setF(
        item
          ? {
              name: item.name,
              code: item.code ?? "",
              legalName: item.legalName ?? "",
              taxId: item.taxId ?? "",
              type: item.supplierType ?? "",
              address: item.address ?? "",
              phone: item.phone ?? "",
              email: item.email ?? "",
              contact: item.contact ?? "",
              registeredOn: isoToDate(item.registeredOn),
              colorHex: item.colorHex ?? "#3b82f6",
              priority: String(item.visualPriority ?? 0),
            }
          : BLANK_SUPPLIER,
      );
    }
  }, [isOpen, item]);

  const set = (k: keyof SupplierForm, v: string) => setF((s) => ({ ...s, [k]: v }));
  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = f.name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Proveedores"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed,
          code: nn(f.code),
          taxId: nn(f.taxId),
          address: nn(f.address),
          supplierType: nn(f.type),
          contact: nn(f.contact),
          legalName: nn(f.legalName),
          phone: nn(f.phone),
          email: nn(f.email),
          registeredOn: dateToIso(f.registeredOn),
          colorHex: nn(f.colorHex),
          visualPriority: Number.parseInt(f.priority, 10) || 0,
        });
      }}
    >
      <Field id="s-name" label="Nombre" required>
        <Input id="s-name" value={f.name} onChange={(e) => set("name", e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <PartyFieldset p="s" f={f} set={set} typeLabel="Tipo de proveedor" />
      <div className="grid grid-cols-2 gap-4">
        <Field id="s-color" label="Color" hint="Color del proveedor en el calendario.">
          <ColorPicker id="s-color" value={f.colorHex} onChange={(v) => set("colorHex", v)} fallback="#3b82f6" />
        </Field>
        <Field id="s-priority" label="Prioridad" hint="Mayor = predomina en el día.">
          <Input id="s-priority" type="number" step="1" value={f.priority} onChange={(e) => set("priority", e.target.value)} placeholder="0" />
        </Field>
      </div>
      <Field id="s-code" label="Código">
        <Input id="s-code" value={f.code} onChange={(e) => set("code", e.target.value)} placeholder="Opcional" maxLength={64} />
      </Field>
    </EditorFrame>
  );
}

export function ProveedoresPage() {
  return (
    <RichCatalogShell<SupplierRow, SupplierInput>
      title="Proveedores"
      unit="proveedor"
      icon={Truck}
      queryKey="suppliers"
      api={supplierCatalog}
      description="Color y prioridad determinan el color del día en el flujo de caja."
      cols="grid-cols-[1fr_120px_90px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "color", label: "Color", get: (it) => it.colorHex },
        { key: "prioridad", label: "Prioridad", get: (it) => it.visualPriority },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <span className="flex items-center gap-2 text-[12px] text-[var(--color-muted-foreground)]">
            <ColorSwatch hex={it.colorHex} />
            <code className="font-mono">{it.colorHex ?? "—"}</code>
          </span>
          <span className="text-[13px] tabular-nums text-[var(--color-foreground)]">{it.visualPriority}</span>
        </>
      )}
      Editor={SupplierEditor}
    />
  );
}

// ══════════════════════ Statuses (type + color) ══════════════════════

const STATUS_TYPES: { value: StatusType; label: string }[] = [
  { value: "Proyecto", label: "Proyecto" },
  { value: "Ingreso", label: "Ingreso" },
  { value: "Pago", label: "Pago" },
  { value: "FacturaEmitida", label: "Factura emitida" },
  { value: "FacturaRecibida", label: "Factura recibida" },
  { value: "ProformaEmitida", label: "Proforma emitida" },
  { value: "ProformaRecibida", label: "Proforma recibida" },
];

function StatusEditor({ state, api, queryKey, onClose }: EditorProps<StatusRow, StatusInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [type, setType] = useState<string | null>(null);
  const [colorHex, setColorHex] = useState("#64748b");

  useEffect(() => {
    if (isOpen) {
      setName(item?.name ?? "");
      setCode(item?.code ?? "");
      setType(item?.type ?? null);
      setColorHex(item?.colorHex ?? "#64748b");
    }
  }, [isOpen, item]);

  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Estados"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed,
          code: code.trim() || null,
          type: (type as StatusType | null) ?? null,
          colorHex: colorHex || null,
        });
      }}
    >
      <Field id="st-name" label="Nombre" required>
        <Input id="st-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="PENDIENTE, PREVISTO…" autoFocus required maxLength={256} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="st-type" label="Tipo" hint="A qué contexto aplica.">
          <Combobox
            id="st-type"
            label="Tipo"
            variant="field"
            value={type}
            onChange={setType}
            options={STATUS_TYPES.map((t) => ({ value: t.value, label: t.label }))}
            clearable
            placeholder="Sin tipo"
            emptyOptionLabel="Sin tipo"
          />
        </Field>
        <Field id="st-code" label="Código">
          <Input id="st-code" value={code} onChange={(e) => setCode(e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
      </div>
      <Field id="st-color" label="Color">
        <div className="flex items-center gap-2">
          <input
            id="st-color"
            type="color"
            value={/^#[0-9a-fA-F]{6}$/.test(colorHex) ? colorHex : "#64748b"}
            onChange={(e) => setColorHex(e.target.value)}
            className="h-9 w-12 cursor-pointer rounded-lg border border-[var(--color-input)] bg-transparent p-1"
          />
          <Input value={colorHex} onChange={(e) => setColorHex(e.target.value)} placeholder="#64748b" maxLength={9} />
        </div>
      </Field>
    </EditorFrame>
  );
}

export function EstadosPage() {
  return (
    <RichCatalogShell<StatusRow, StatusInput>
      title="Estados"
      unit="estado"
      icon={ListChecks}
      queryKey="statuses"
      api={statusCatalog}
      description="Catálogo polimórfico por tipo (proyectos, ingresos, pagos, facturas y proformas)."
      cols="grid-cols-[1fr_120px_110px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "tipo", label: "Tipo", get: (it) => it.type },
        { key: "color", label: "Color", get: (it) => it.colorHex },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <span>{it.type ? <EntityStatusBadge>{it.type}</EntityStatusBadge> : <span className="text-[var(--color-muted-foreground)]">—</span>}</span>
          <span className="flex items-center gap-2 text-[12px] text-[var(--color-muted-foreground)]">
            <ColorSwatch hex={it.colorHex} />
            <code className="font-mono">{it.colorHex ?? "—"}</code>
          </span>
        </>
      )}
      Editor={StatusEditor}
    />
  );
}

// ══════════════════════ Societies (child of Client) ══════════════════════

function useClientOptions(enabled: boolean) {
  return useQuery({
    queryKey: ["administration", "clients", "options"],
    queryFn: () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled,
  });
}

function SocietyEditor({ state, api, queryKey, onClose }: EditorProps<SocietyRow, SocietyInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [name, setName] = useState("");
  const [taxId, setTaxId] = useState("");
  const [address, setAddress] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [city, setCity] = useState("");
  const [country, setCountry] = useState("");
  const [clientId, setClientId] = useState<string | null>(null);

  const clientsQ = useClientOptions(isOpen);

  useEffect(() => {
    if (isOpen) {
      setName(item?.name ?? "");
      setTaxId(item?.taxId ?? "");
      setAddress(item?.address ?? "");
      setPostalCode(item?.postalCode ?? "");
      setCity(item?.city ?? "");
      setCountry(item?.country ?? "");
      setClientId(item?.clientId ?? null);
    }
  }, [isOpen, item]);

  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Sociedades"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed,
          taxId: taxId.trim() || null,
          address: address.trim() || null,
          postalCode: postalCode.trim() || null,
          city: city.trim() || null,
          country: country.trim() || null,
          clientId,
        });
      }}
    >
      <Field id="so-name" label="Nombre" required>
        <Input id="so-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <Field id="so-client" label="Cliente">
        <Combobox
          id="so-client"
          label="Cliente"
          variant="field"
          value={clientId}
          onChange={setClientId}
          options={toOptions(clientsQ.data?.items)}
          searchable
          clearable
          placeholder={clientsQ.isLoading ? "Cargando…" : "Sin asignar"}
          emptyOptionLabel="Sin asignar"
        />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="so-taxid" label="NIF/CIF">
          <Input id="so-taxid" value={taxId} onChange={(e) => setTaxId(e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
        <Field id="so-postal" label="Código postal">
          <Input id="so-postal" value={postalCode} onChange={(e) => setPostalCode(e.target.value)} placeholder="Opcional" maxLength={32} />
        </Field>
      </div>
      <Field id="so-address" label="Dirección">
        <Input id="so-address" value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Opcional" maxLength={512} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="so-city" label="Ciudad">
          <Input id="so-city" value={city} onChange={(e) => setCity(e.target.value)} placeholder="Opcional" maxLength={128} />
        </Field>
        <Field id="so-country" label="País">
          <Input id="so-country" value={country} onChange={(e) => setCountry(e.target.value)} placeholder="Opcional" maxLength={128} />
        </Field>
      </div>
    </EditorFrame>
  );
}

export function SociedadesPage() {
  return (
    <RichCatalogShell<SocietyRow, SocietyInput>
      title="Sociedades"
      unit="sociedad"
      icon={Building2}
      queryKey="societies"
      api={societiesApi}
      description="Sociedades pertenecientes a un cliente."
      cols="grid-cols-[1fr_140px_140px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "nif", label: "NIF/CIF", get: (it) => it.taxId },
        { key: "ciudad", label: "Ciudad", get: (it) => it.city },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <code className="truncate font-mono text-[12px] text-[var(--color-muted-foreground)]">{it.taxId ?? "—"}</code>
          <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">{it.city ?? "—"}</span>
        </>
      )}
      Editor={SocietyEditor}
    />
  );
}

// ══════════════════════ Prefixes (hierarchical GRUPO → CATEGORIA) ══════════════════════

const PREFIX_TYPES: { value: PrefixType; label: string }[] = [
  { value: "Grupo", label: "Grupo" },
  { value: "Categoria", label: "Categoría" },
];

function useGroupOptions(enabled: boolean) {
  return useQuery({
    queryKey: ["administration", "prefixes", "groups"],
    queryFn: () => prefixesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc", type: "Grupo" }),
    enabled,
  });
}

function PrefixEditor({ state, api, queryKey, onClose }: EditorProps<PrefixRow, PrefixInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [type, setType] = useState<PrefixType>("Categoria");
  const [groupId, setGroupId] = useState<string | null>(null);
  const [isActive, setIsActive] = useState(true);

  const groupsQ = useGroupOptions(isOpen);

  useEffect(() => {
    if (isOpen) {
      setName(item?.name ?? "");
      setDescription(item?.description ?? "");
      setType(item?.type ?? "Categoria");
      setGroupId(item?.prefixGroupId ?? null);
      setIsActive(item?.isActive ?? true);
    }
  }, [isOpen, item]);

  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = name.trim();
  const groupOptions: ComboboxOption[] = (groupsQ.data?.items ?? [])
    .filter((g) => g.id !== item?.id)
    .map((g) => ({ value: g.id, label: g.name }));

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Prefijos"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed,
          description: description.trim() || null,
          prefixGroupId: type === "Categoria" ? groupId : null,
          type,
          isActive,
        });
      }}
    >
      <Field id="pf-name" label="Nombre" required>
        <Input id="pf-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <Field id="pf-type" label="Tipo" required>
        <Combobox
          id="pf-type"
          label="Tipo"
          variant="field"
          value={type}
          onChange={(v) => setType((v as PrefixType) ?? "Categoria")}
          options={PREFIX_TYPES.map((t) => ({ value: t.value, label: t.label }))}
          placeholder="Tipo"
        />
      </Field>
      {type === "Categoria" && (
        <Field id="pf-group" label="Grupo" hint="Grupo al que pertenece la categoría.">
          <Combobox
            id="pf-group"
            label="Grupo"
            variant="field"
            value={groupId}
            onChange={setGroupId}
            options={groupOptions}
            searchable
            clearable
            placeholder={groupsQ.isLoading ? "Cargando…" : "Sin grupo"}
            emptyOptionLabel="Sin grupo"
          />
        </Field>
      )}
      <Field id="pf-desc" label="Descripción">
        <Input id="pf-desc" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Opcional" maxLength={1024} />
      </Field>
      <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2">
        <span className="text-[13px] text-[var(--color-foreground)]">Activo</span>
        <Switch checked={isActive} onCheckedChange={setIsActive} aria-label="Activo" />
      </div>
    </EditorFrame>
  );
}

export function PrefijosPage() {
  return (
    <RichCatalogShell<PrefixRow, PrefixInput>
      title="Prefijos"
      unit="prefijo"
      icon={Hash}
      queryKey="prefixes"
      api={prefixesApi}
      description="Categorización jerárquica de proyectos (Grupo → Categoría)."
      cols="grid-cols-[1fr_110px_90px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "tipo", label: "Tipo", get: (it) => (it.type === "Grupo" ? "Grupo" : "Categoría") },
        { key: "activo", label: "Activo", get: (it) => (it.isActive ? "Activo" : "Inactivo") },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <span>
            <EntityStatusBadge tone={it.type === "Grupo" ? "info" : "default"}>
              {it.type === "Grupo" ? "Grupo" : "Categoría"}
            </EntityStatusBadge>
          </span>
          <span>
            {it.isActive ? (
              <EntityStatusBadge tone="success" withDot>
                Activo
              </EntityStatusBadge>
            ) : (
              <span className="text-[12px] text-[var(--color-muted-foreground)]">Inactivo</span>
            )}
          </span>
        </>
      )}
      Editor={PrefixEditor}
    />
  );
}

// ══════════════════════ Clients (rich CRM) ══════════════════════

type ClientForm = PartyForm & { name: string; code: string };
const BLANK_CLIENT: ClientForm = {
  name: "", code: "", legalName: "", taxId: "", type: "", address: "", phone: "", email: "", contact: "", registeredOn: "", colorHex: "",
};

function ClientEditor({ state, api, queryKey, onClose }: EditorProps<ClientRow, ClientInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [f, setF] = useState<ClientForm>(BLANK_CLIENT);

  useEffect(() => {
    if (isOpen) {
      setF(
        item
          ? {
              name: item.name,
              code: item.code ?? "",
              legalName: item.legalName ?? "",
              taxId: item.taxId ?? "",
              type: item.clientType ?? "",
              address: item.address ?? "",
              phone: item.phone ?? "",
              email: item.email ?? "",
              contact: item.contact ?? "",
              registeredOn: isoToDate(item.registeredOn),
              colorHex: item.colorHex ?? "",
            }
          : BLANK_CLIENT,
      );
    }
  }, [isOpen, item]);

  const set = (k: keyof ClientForm, v: string) => setF((s) => ({ ...s, [k]: v }));
  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = f.name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Clientes"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed,
          code: nn(f.code),
          taxId: nn(f.taxId),
          address: nn(f.address),
          clientType: nn(f.type),
          contact: nn(f.contact),
          legalName: nn(f.legalName),
          phone: nn(f.phone),
          email: nn(f.email),
          registeredOn: dateToIso(f.registeredOn),
          colorHex: nn(f.colorHex),
        });
      }}
    >
      <Field id="c-name" label="Nombre" required>
        <Input id="c-name" value={f.name} onChange={(e) => set("name", e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <PartyFieldset p="c" f={f} set={set} typeLabel="Tipo de cliente" />
      <div className="grid grid-cols-2 gap-4">
        <Field id="c-color" label="Color">
          <ColorPicker id="c-color" value={f.colorHex} onChange={(v) => set("colorHex", v)} fallback="#3b82f6" />
        </Field>
        <Field id="c-code" label="Código">
          <Input id="c-code" value={f.code} onChange={(e) => set("code", e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
      </div>
    </EditorFrame>
  );
}

export function ClientesPage() {
  return (
    <RichCatalogShell<ClientRow, ClientInput>
      title="Clientes"
      unit="cliente"
      icon={Users}
      queryKey="clients"
      api={clientCatalog}
      description="Clientes con sus datos fiscales y de contacto."
      cols="grid-cols-[1fr_150px_140px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "nif", label: "NIF/CIF", get: (it) => it.taxId },
        { key: "contacto", label: "Contacto", get: (it) => it.contact },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <code className="truncate font-mono text-[12px] text-[var(--color-muted-foreground)]">{it.taxId ?? "—"}</code>
          <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">{it.contact ?? "—"}</span>
        </>
      )}
      Editor={ClientEditor}
    />
  );
}

// ══════════════════════ Companies (billing entities) ══════════════════════

function CompanyEditor({ state, api, queryKey, onClose }: EditorProps<CompanyRow, CompanyInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [legalName, setLegalName] = useState("");
  const [taxRegistration, setTaxRegistration] = useState("");
  const [nif, setNif] = useState("");
  const [address, setAddress] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [city, setCity] = useState("");
  const [phone, setPhone] = useState("");
  const [email, setEmail] = useState("");
  const [showInProjects, setShowInProjects] = useState(true);

  useEffect(() => {
    if (isOpen) {
      setName(item?.name ?? "");
      setCode(item?.code ?? "");
      setLegalName(item?.legalName ?? "");
      setTaxRegistration(item?.taxRegistration ?? "");
      setNif(item?.nif ?? "");
      setAddress(item?.address ?? "");
      setPostalCode(item?.postalCode ?? "");
      setCity(item?.city ?? "");
      setPhone(item?.phone ?? "");
      setEmail(item?.email ?? "");
      setShowInProjects(item?.showInProjects ?? true);
    }
  }, [isOpen, item]);

  // Logo — subida directa (solo en edición: hace falta el id de la empresa).
  const queryClient = useQueryClient();
  const logoInputRef = useRef<HTMLInputElement>(null);
  const logoUpload = useMutation({
    mutationFn: ({ id, file }: { id: string; file: File }) => uploadCompanyLogo(id, file),
    onSuccess: () => {
      toast.success("Logo guardado — aparecerá en el PDF de las facturas");
      queryClient.invalidateQueries({ queryKey: ["administration", "companies"] });
    },
    onError: (err) => toast.error("No se pudo subir el logo", { description: describe(err) }),
  });

  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Empresas"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed, code: nn(code), legalName: nn(legalName), taxRegistration: nn(taxRegistration),
          showInProjects, nif: nn(nif), address: nn(address), postalCode: nn(postalCode), city: nn(city),
          phone: nn(phone), email: nn(email),
        });
      }}
    >
      <Field id="e-name" label="Nombre" required>
        <Input id="e-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <Field id="e-legal" label="Razón social">
        <Input id="e-legal" value={legalName} onChange={(e) => setLegalName(e.target.value)} placeholder="Opcional" maxLength={256} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="e-reg" label="Registro fiscal">
          <Input id="e-reg" value={taxRegistration} onChange={(e) => setTaxRegistration(e.target.value)} placeholder="Opcional" maxLength={128} />
        </Field>
        <Field id="e-code" label="Código">
          <Input id="e-code" value={code} onChange={(e) => setCode(e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
      </div>
      <Field id="e-nif" label="NIF" hint="Obligatorio para emitir con VeriFactu (registro AEAT por empresa).">
        <Input id="e-nif" value={nif} onChange={(e) => setNif(e.target.value)} placeholder="B12345678" maxLength={20} />
      </Field>
      <Field id="e-address" label="Dirección" hint="Aparece en la cabecera del PDF de las facturas.">
        <Input id="e-address" value={address} onChange={(e) => setAddress(e.target.value)} placeholder="Calle y número" maxLength={512} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="e-cp" label="Código postal">
          <Input id="e-cp" value={postalCode} onChange={(e) => setPostalCode(e.target.value)} placeholder="28001" maxLength={32} />
        </Field>
        <Field id="e-city" label="Ciudad">
          <Input id="e-city" value={city} onChange={(e) => setCity(e.target.value)} placeholder="Madrid" maxLength={128} />
        </Field>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <Field id="e-phone" label="Teléfono">
          <Input id="e-phone" value={phone} onChange={(e) => setPhone(e.target.value)} placeholder="Opcional" maxLength={64} />
        </Field>
        <Field id="e-email" label="Email">
          <Input id="e-email" type="email" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Opcional" maxLength={256} />
        </Field>
      </div>
      {item && (
        <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2">
          <div className="min-w-0">
            <p className="text-[13px] text-[var(--color-foreground)]">Logo</p>
            <p className="truncate text-[11.5px] text-[var(--color-muted-foreground)]">
              {item.logoPath ? "Hay un logo guardado — se incrusta en el PDF." : "Imagen PNG/JPG, máx. 1 MB."}
            </p>
          </div>
          <Button
            type="button"
            variant="outline"
            disabled={logoUpload.isPending}
            onClick={() => logoInputRef.current?.click()}
            className="h-8 rounded-lg px-3 text-[12.5px]"
          >
            {logoUpload.isPending ? "Subiendo…" : item.logoPath ? "Reemplazar" : "Subir logo"}
          </Button>
          <input
            ref={logoInputRef}
            type="file"
            accept="image/png,image/jpeg"
            className="hidden"
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (file && item) logoUpload.mutate({ id: item.id, file });
              e.target.value = "";
            }}
          />
        </div>
      )}
      <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2">
        <div>
          <p className="text-[13px] text-[var(--color-foreground)]">Visible en proyectos</p>
          <p className="text-[11.5px] text-[var(--color-muted-foreground)]">Aparece como tarjeta en la pantalla de Empresas de Proyectos.</p>
        </div>
        <Switch checked={showInProjects} onCheckedChange={setShowInProjects} aria-label="Visible en proyectos" />
      </div>
    </EditorFrame>
  );
}

// Card grid: each company links to its projects; "+" creates, "Todos" shows every project.
export function EmpresasPage() {
  const navigate = useNavigate();
  const [editor, setEditor] = useState<EditorState<CompanyRow>>({ mode: "closed" });

  const q = useQuery({
    queryKey: ["administration", "companies", "cards"],
    queryFn: () => companyCatalog.search({ pageSize: 10000, sortBy: "name", sortDir: "asc", showInProjects: true }),
  });

  const items = q.data?.items ?? [];
  const cardBase =
    "group relative flex min-h-[110px] cursor-pointer flex-col items-center justify-center gap-2 rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 text-center transition-all hover:-translate-y-0.5 hover:border-[var(--color-primary)] hover:shadow-md";

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Building}
        title="Empresas"
        total={q.data?.totalCount ?? null}
        unit="empresa"
        description="Seleccione una empresa para ver sus proyectos."
      />

      {q.isLoading ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {Array.from({ length: 6 }, (_, i) => (
            <div key={i} className="skeleton min-h-[110px] rounded-xl" aria-hidden />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          <button
            type="button"
            onClick={() => setEditor({ mode: "create" })}
            aria-label="Nueva empresa"
            className={`${cardBase} border-dashed`}
          >
            <Plus className="size-7 text-[var(--color-primary)]" />
            <span className="text-[13px] font-medium text-[var(--color-muted-foreground)]">Nueva empresa</span>
          </button>

          <button
            type="button"
            onClick={() => navigate("/projects")}
            aria-label="Ver todos los proyectos"
            className={cardBase}
          >
            <Building2 className="size-6 text-[var(--color-primary)]" />
            <span className="text-[14px] font-semibold tracking-wide text-[var(--color-foreground)]">Todos</span>
            <span className="text-[12px] text-[var(--color-muted-foreground)]">Proyectos de todas las empresas</span>
          </button>

          {items.map((it) => (
            <button
              key={it.id}
              type="button"
              onClick={() => navigate(`/projects?empresa=${encodeURIComponent(it.id)}`)}
              aria-label={`Ver proyectos de ${it.name}`}
              className={cardBase}
            >
              <span className="text-[14px] font-semibold uppercase leading-snug text-[var(--color-foreground)] transition-colors group-hover:text-[var(--color-primary)]">
                {it.name}
              </span>
              {it.legalName && (
                <span className="truncate text-[12px] text-[var(--color-muted-foreground)]">{it.legalName}</span>
              )}
              <span className="absolute right-1.5 top-1.5 flex gap-1 opacity-0 transition-opacity group-hover:opacity-100">
                <span
                  role="button"
                  tabIndex={0}
                  aria-label={`Editar ${it.name}`}
                  onClick={(e) => {
                    e.stopPropagation();
                    setEditor({ mode: "edit", item: it });
                  }}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      e.stopPropagation();
                      setEditor({ mode: "edit", item: it });
                    }
                  }}
                  className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)]"
                >
                  <Pencil className="size-3.5" />
                </span>
                <span
                  role="button"
                  tabIndex={0}
                  aria-label={`Borrar ${it.name}`}
                  onClick={(e) => {
                    e.stopPropagation();
                    setEditor({ mode: "delete", item: it });
                  }}
                  onKeyDown={(e) => {
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      e.stopPropagation();
                      setEditor({ mode: "delete", item: it });
                    }
                  }}
                  className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"
                >
                  <Trash2 className="size-3.5" />
                </span>
              </span>
            </button>
          ))}
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

      <CompanyEditor state={editor} api={companyCatalog} queryKey="companies" onClose={() => setEditor({ mode: "closed" })} />
      <DeleteDialog state={editor} api={companyCatalog} queryKey="companies" onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

// Plain CRUD list of ALL companies (Administración) — the Proyectos screen only shows the visible ones.
export function EmpresasAdminPage() {
  return (
    <RichCatalogShell<CompanyRow, CompanyInput>
      title="Empresas"
      unit="empresa"
      icon={Building}
      queryKey="companies"
      api={companyCatalog}
      description="Entidades emisoras / de facturación. «En proyectos» controla si aparecen como tarjeta en Proyectos."
      cols="grid-cols-[1fr_180px_140px_110px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "razon", label: "Razón social", get: (it) => it.legalName },
        { key: "registro", label: "Registro fiscal", get: (it) => it.taxRegistration },
        { key: "proyectos", label: "En proyectos", get: (it) => (it.showInProjects ? "Visible" : "Oculta") },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">{it.legalName ?? "—"}</span>
          <code className="truncate font-mono text-[12px] text-[var(--color-muted-foreground)]">{it.taxRegistration ?? "—"}</code>
          <span>
            {it.showInProjects ? (
              <EntityStatusBadge tone="success" withDot>
                Visible
              </EntityStatusBadge>
            ) : (
              <span className="text-[12px] text-[var(--color-muted-foreground)]">Oculta</span>
            )}
          </span>
        </>
      )}
      Editor={CompanyEditor}
    />
  );
}

// ══════════════════════ Countries (name + ISO + description) ══════════════════════

function CountryEditor({ state, api, queryKey, onClose }: EditorProps<CountryRow, CountryInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [description, setDescription] = useState("");

  useEffect(() => {
    if (isOpen) {
      setName(item?.name ?? "");
      setCode(item?.code ?? "");
      setDescription(item?.description ?? "");
    }
  }, [isOpen, item]);

  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Países"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({ name: trimmed, code: nn(code), description: nn(description) });
      }}
    >
      <Field id="pa-name" label="Nombre" required>
        <Input id="pa-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <Field id="pa-code" label="Código ISO">
        <Input id="pa-code" value={code} onChange={(e) => setCode(e.target.value)} placeholder="ES, PT, IT…" maxLength={64} />
      </Field>
      <Field id="pa-desc" label="Descripción">
        <Input id="pa-desc" value={description} onChange={(e) => setDescription(e.target.value)} placeholder="Opcional" maxLength={1024} />
      </Field>
    </EditorFrame>
  );
}

export function PaisesPage() {
  return (
    <RichCatalogShell<CountryRow, CountryInput>
      title="Países"
      unit="país"
      icon={Globe}
      queryKey="countries"
      api={countryCatalog}
      description="Países con su código ISO."
      cols="grid-cols-[1fr_110px_1fr_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "codigo", label: "Código ISO", get: (it) => it.code },
        { key: "desc", label: "Descripción", get: (it) => it.description },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <code className="truncate font-mono text-[12px] text-[var(--color-muted-foreground)]">{it.code ?? "—"}</code>
          <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">{it.description ?? "—"}</span>
        </>
      )}
      Editor={CountryEditor}
    />
  );
}

// ══════════════════════ Banks (statement-import column mapping — §33) ══════════════════════

type BankForm = {
  name: string;
  startRow: string;
  dateColumn: string;
  conceptColumn: string;
  amountColumn: string;
  balanceColumn: string;
  notes: string;
  isActive: boolean;
};
const BLANK_BANK: BankForm = {
  name: "", startRow: "1", dateColumn: "0", conceptColumn: "1", amountColumn: "2", balanceColumn: "3", notes: "", isActive: true,
};
const toInt = (s: string) => Number.parseInt(s, 10) || 0;

function BankEditor({ state, api, queryKey, onClose }: EditorProps<BankRow, BankInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [f, setF] = useState<BankForm>(BLANK_BANK);

  useEffect(() => {
    if (isOpen) {
      setF(
        item
          ? {
              name: item.name,
              startRow: String(item.startRow),
              dateColumn: String(item.dateColumn),
              conceptColumn: String(item.conceptColumn),
              amountColumn: String(item.amountColumn),
              balanceColumn: String(item.balanceColumn),
              notes: item.notes ?? "",
              isActive: item.isActive,
            }
          : BLANK_BANK,
      );
    }
  }, [isOpen, item]);

  const setV = (k: Exclude<keyof BankForm, "isActive">, v: string) => setF((s) => ({ ...s, [k]: v }));
  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = f.name.trim();

  return (
    <EditorFrame
      isOpen={isOpen}
      editing={!!item}
      title="Bancos"
      saving={save.isPending}
      canSave={!!trimmed}
      onClose={onClose}
      onSubmit={(e) => {
        e.preventDefault();
        if (!trimmed) return;
        save.mutate({
          name: trimmed,
          startRow: toInt(f.startRow),
          dateColumn: toInt(f.dateColumn),
          conceptColumn: toInt(f.conceptColumn),
          amountColumn: toInt(f.amountColumn),
          balanceColumn: toInt(f.balanceColumn),
          isActive: f.isActive,
          notes: nn(f.notes),
        });
      }}
    >
      <Field id="b-name" label="Nombre" required>
        <Input id="b-name" value={f.name} onChange={(e) => setV("name", e.target.value)} placeholder="Nombre del banco" autoFocus required maxLength={256} />
      </Field>
      <Field id="b-startrow" label="Fila de inicio" hint="Fila del extracto donde empiezan los datos.">
        <Input id="b-startrow" type="number" step="1" min="0" value={f.startRow} onChange={(e) => setV("startRow", e.target.value)} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="b-datecol" label="Columna fecha">
          <Input id="b-datecol" type="number" step="1" min="0" value={f.dateColumn} onChange={(e) => setV("dateColumn", e.target.value)} />
        </Field>
        <Field id="b-conceptcol" label="Columna concepto">
          <Input id="b-conceptcol" type="number" step="1" min="0" value={f.conceptColumn} onChange={(e) => setV("conceptColumn", e.target.value)} />
        </Field>
      </div>
      <div className="grid grid-cols-2 gap-4">
        <Field id="b-amountcol" label="Columna importe">
          <Input id="b-amountcol" type="number" step="1" min="0" value={f.amountColumn} onChange={(e) => setV("amountColumn", e.target.value)} />
        </Field>
        <Field id="b-balancecol" label="Columna saldo">
          <Input id="b-balancecol" type="number" step="1" min="0" value={f.balanceColumn} onChange={(e) => setV("balanceColumn", e.target.value)} />
        </Field>
      </div>
      <Field id="b-notes" label="Notas">
        <Input id="b-notes" value={f.notes} onChange={(e) => setV("notes", e.target.value)} placeholder="Opcional" maxLength={1024} />
      </Field>
      <div className="flex items-center justify-between rounded-lg border border-[var(--color-border)] px-3 py-2">
        <span className="text-[13px] text-[var(--color-foreground)]">Activo</span>
        <Switch checked={f.isActive} onCheckedChange={(v) => setF((s) => ({ ...s, isActive: v }))} aria-label="Activo" />
      </div>
    </EditorFrame>
  );
}

export function BancosPage() {
  return (
    <RichCatalogShell<BankRow, BankInput>
      title="Bancos"
      unit="banco"
      icon={Landmark}
      queryKey="banks"
      api={bankCatalog}
      description="Mapeo de columnas para importar extractos bancarios."
      cols="grid-cols-[1fr_100px_90px_48px]"
      columns={[
        { key: "name", label: "Nombre", get: (it) => it.name },
        { key: "fila", label: "Fila inicio", get: (it) => it.startRow },
        { key: "activo", label: "Activo", get: (it) => (it.isActive ? "Activo" : "Inactivo") },
      ]}
      rowCells={(it) => (
        <>
          <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
          <span className="text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{it.startRow}</span>
          <span>
            {it.isActive ? (
              <EntityStatusBadge tone="success" withDot>
                Activo
              </EntityStatusBadge>
            ) : (
              <span className="text-[12px] text-[var(--color-muted-foreground)]">Inactivo</span>
            )}
          </span>
        </>
      )}
      Editor={BankEditor}
    />
  );
}

// ══════════════════════ Bank movements (imported statement lines — read-only) ══════════════════════

const fmtMoney = (n: number) => new Intl.NumberFormat("es-ES", { maximumFractionDigits: 2 }).format(n);

export function MovimientosBancosPage() {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  // Sort/filtro por columna + paginación. Con un orden o filtro activo la
  // consulta se ensancha al dataset completo y la tabla pagina en memoria.
  const ctl = useTableState({ pageSize: PAGE_SIZE });
  const { setPage } = ctl;

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPage(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search, setPage]);

  const q = useQuery({
    queryKey: ["administration", "bank-movements", { search: debouncedSearch, ...ctl.fetch }],
    queryFn: () => bankMovementsApi.search({ search: debouncedSearch || undefined, ...ctl.fetch, sortDir: "desc" }),
    placeholderData: keepPreviousData,
  });

  const data = q.data;
  const view = useTableRows<BankMovementRow>(
    data?.items ?? [],
    {
      fecha: (it) => it.date,
      concepto: (it) => it.concept,
      importe: (it) => it.amount,
      saldo: (it) => it.balance,
      banco: (it) => it.bankName,
    },
    ctl,
    data,
  );
  const items: BankMovementRow[] = view.rows;
  const cols = "grid-cols-[110px_1fr_120px_120px_150px]";

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Banknote}
        title="Movimientos de banco"
        total={view.totalCount}
        unit="movimiento"
        description="Líneas importadas de extractos bancarios."
      />
      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por concepto o banco…" />

      {q.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={cols} />
      ) : items.length === 0 && !ctl.hasActiveFilters ? (
        <EntityEmpty
          icon={Banknote}
          title="Sin movimientos"
          body="Aún no hay movimientos importados. El mapeo de columnas se configura en Bancos."
        />
      ) : (
        <div>
          <EntityListCard className="hidden md:block">
            <EntityListHeader className={cols}>
              {(
                [
                  ["fecha", "Fecha"],
                  ["concepto", "Concepto"],
                  ["importe", "Importe"],
                  ["saldo", "Saldo"],
                  ["banco", "Banco"],
                ] as const
              ).map(([key, label]) => (
                <EntityColHeader
                  key={key}
                  colKey={key}
                  label={label}
                  sort={ctl.sort}
                  filters={ctl.filters}
                  onSort={ctl.toggleSort}
                  onFilter={ctl.setFilter}
                />
              ))}
            </EntityListHeader>
            {items.length === 0 && <EntityFilterEmptyRow onClear={ctl.clearFilters} />}
            {items.map((it, i) => (
              <EntityListRow key={it.id} className={cols} isLast={i === items.length - 1}>
                <span className="text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{it.date ? it.date.slice(0, 10) : "—"}</span>
                <span className="truncate text-[14px] text-[var(--color-foreground)]">{it.concept}</span>
                <span className="text-[13px] tabular-nums text-[var(--color-foreground)]">{fmtMoney(it.amount)}</span>
                <span className="text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{fmtMoney(it.balance)}</span>
                <span className="truncate text-[13px] text-[var(--color-muted-foreground)]">{it.bankName}</span>
              </EntityListRow>
            ))}
          </EntityListCard>

          <div className="space-y-2 md:hidden">
            {items.map((it) => (
              <EntityMobileCard key={it.id} href="#" onClick={(e) => e.preventDefault()}>
                <div className="min-w-0">
                  <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.concept}</p>
                  <p className="truncate text-[12px] text-[var(--color-muted-foreground)]">
                    {(it.date ? it.date.slice(0, 10) : "—") + " · " + it.bankName + " · " + fmtMoney(it.amount)}
                  </p>
                </div>
              </EntityMobileCard>
            ))}
          </div>

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
    </div>
  );
}
