import { useEffect, useState, type FormEvent, type ReactNode } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Building2, Hash, ListChecks, Pencil, Plus, Search, Trash2, Truck, type LucideIcon } from "lucide-react";
import { toast } from "sonner";
import {
  clientsApi,
  prefixesApi,
  societiesApi,
  statusCatalog,
  supplierCatalog,
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
function RichCatalogShell<TRow extends Base, TInput>({
  title,
  unit,
  icon,
  queryKey,
  api,
  description,
  cols,
  headerCells,
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
  headerCells: ReactNode;
  rowCells: (item: TRow) => ReactNode;
  Editor: (props: EditorProps<TRow, TInput>) => ReactNode;
}) {
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [editor, setEditor] = useState<EditorState<TRow>>({ mode: "closed" });

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPageNumber(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search]);

  const q = useQuery({
    queryKey: ["administration", queryKey, { search: debouncedSearch, pageNumber }],
    queryFn: () =>
      api.search({ search: debouncedSearch || undefined, pageNumber, pageSize: PAGE_SIZE, sortBy: "name", sortDir: "asc" }),
    placeholderData: keepPreviousData,
  });

  const data = q.data;
  const items = data?.items ?? [];
  const searchActive = debouncedSearch.length > 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader icon={icon} title={title} total={data?.totalCount ?? null} unit={unit} description={description}>
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
      ) : items.length === 0 ? (
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
              {headerCells}
              <span />
            </EntityListHeader>
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
            page={data?.pageNumber ?? 1}
            totalPages={data?.totalPages ?? 1}
            hasPrev={!!data?.hasPrevious}
            hasNext={!!data?.hasNext}
            onPrev={() => setPageNumber((p) => Math.max(1, p - 1))}
            onNext={() => setPageNumber((p) => p + 1)}
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

// ══════════════════════ Suppliers (color + priority) ══════════════════════

function SupplierEditor({ state, api, queryKey, onClose }: EditorProps<SupplierRow, SupplierInput>) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const [name, setName] = useState("");
  const [code, setCode] = useState("");
  const [colorHex, setColorHex] = useState("#3b82f6");
  const [priority, setPriority] = useState("0");

  useEffect(() => {
    if (isOpen) {
      setName(item?.name ?? "");
      setCode(item?.code ?? "");
      setColorHex(item?.colorHex ?? "#3b82f6");
      setPriority((item?.visualPriority ?? 0).toString());
    }
  }, [isOpen, item]);

  const save = useSaveMutation(api, queryKey, item, onClose);
  const trimmed = name.trim();

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
          code: code.trim() || null,
          colorHex: colorHex || null,
          visualPriority: Number.parseInt(priority, 10) || 0,
        });
      }}
    >
      <Field id="s-name" label="Nombre" required>
        <Input id="s-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
      </Field>
      <Field id="s-code" label="Código">
        <Input id="s-code" value={code} onChange={(e) => setCode(e.target.value)} placeholder="Opcional" maxLength={64} />
      </Field>
      <div className="grid grid-cols-2 gap-4">
        <Field id="s-color" label="Color" hint="Color del proveedor en el calendario.">
          <div className="flex items-center gap-2">
            <input
              id="s-color"
              type="color"
              value={/^#[0-9a-fA-F]{6}$/.test(colorHex) ? colorHex : "#3b82f6"}
              onChange={(e) => setColorHex(e.target.value)}
              className="h-9 w-12 cursor-pointer rounded-lg border border-[var(--color-input)] bg-transparent p-1"
            />
            <Input value={colorHex} onChange={(e) => setColorHex(e.target.value)} placeholder="#3b82f6" maxLength={9} />
          </div>
        </Field>
        <Field id="s-priority" label="Prioridad" hint="Mayor = predomina en el día.">
          <Input id="s-priority" type="number" step="1" value={priority} onChange={(e) => setPriority(e.target.value)} placeholder="0" />
        </Field>
      </div>
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
      headerCells={
        <>
          <span>Nombre</span>
          <span>Color</span>
          <span>Prioridad</span>
        </>
      }
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
      description="Catálogo polimórfico por tipo (Proyecto / Ingreso / Pago)."
      cols="grid-cols-[1fr_120px_110px_48px]"
      headerCells={
        <>
          <span>Nombre</span>
          <span>Tipo</span>
          <span>Color</span>
        </>
      }
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
    queryFn: () => clientsApi.search({ pageSize: 200, sortBy: "name", sortDir: "asc" }),
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
      headerCells={
        <>
          <span>Nombre</span>
          <span>NIF/CIF</span>
          <span>Ciudad</span>
        </>
      }
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
    queryFn: () => prefixesApi.search({ pageSize: 200, sortBy: "name", sortDir: "asc", type: "Grupo" }),
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
      headerCells={
        <>
          <span>Nombre</span>
          <span>Tipo</span>
          <span>Activo</span>
        </>
      }
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
