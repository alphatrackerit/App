import { useEffect, useMemo, useState, type FormEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Pencil, Plus, Search, Send, ShoppingCart, Trash2, X } from "lucide-react";
import { toast } from "sonner";
import {
  cambiarEstadoPedido,
  createPedido,
  deletePedido,
  searchGalpones,
  searchLotes,
  searchPedidos,
  updatePedido,
  ESTADO_PEDIDO,
  TIPO_PEDIDO,
  type EstadoPedido,
  type PedidoDto,
  type PedidoInput,
  type TipoPedido,
} from "@/api/avicola";
import { suppliersApi } from "@/api/administration";
import { Button } from "@/components/ui/button";
import {
  Dialog, DialogBody, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import {
  Combobox, type ComboboxOption,
  EntityEmpty, EntityListCard, EntityListHeader, EntityListLoading, EntityListRow,
  EntityPageHeader, EntityPager, EntitySearch, Field,
} from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

const PAGE_SIZE = 20;
const COLS = "grid-cols-[1fr_120px_110px_120px_150px]";

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
function isoToDateInput(iso: string | null | undefined): string {
  if (!iso) return "";
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? "" : d.toISOString().slice(0, 10);
}

const ESTADO_TONE: Record<EstadoPedido, string> = {
  Borrador: "text-[var(--color-muted-foreground)]",
  Enviado: "text-[var(--color-foreground)]",
  Recibido: "text-[var(--color-primary)]",
  Cancelado: "text-[var(--color-destructive)]",
};

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; pedido: PedidoDto }
  | { mode: "delete"; pedido: PedidoDto };

export function PedidosPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [estado, setEstado] = useState<EstadoPedido | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [editor, setEditor] = useState<EditorState>({ mode: "closed" });

  useEffect(() => {
    const t = setTimeout(() => { setDebouncedSearch(search.trim()); setPageNumber(1); }, 250);
    return () => clearTimeout(t);
  }, [search]);

  const query = useQuery({
    queryKey: ["avicola", "pedidos", "list", { search: debouncedSearch, estado, pageNumber }],
    queryFn: () => searchPedidos({
      search: debouncedSearch || undefined,
      estado: estado ?? undefined,
      pageNumber, pageSize: PAGE_SIZE, sortBy: "fechaPedido", sortDir: "desc",
    }),
    placeholderData: keepPreviousData,
  });

  const cambiar = useMutation({
    mutationFn: ({ id, accion }: { id: string; accion: "Enviar" | "Recibir" | "Cancelar" }) =>
      cambiarEstadoPedido(id, accion),
    onSuccess: () => { toast.success("Pedido actualizado"); queryClient.invalidateQueries({ queryKey: ["avicola", "pedidos"] }); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const data = query.data;
  const items = data?.items ?? [];
  const searchActive = debouncedSearch.length > 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={ShoppingCart}
        title="Pedidos"
        total={data?.totalCount ?? null}
        unit="pedido"
        description="Control de pedidos a proveedores: pienso, pollitos, medicamentos e insumos, con su recepción."
      >
        <Button onClick={() => setEditor({ mode: "create" })} className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none">
          <Plus className="size-4" />
          Nuevo pedido
        </Button>
      </EntityPageHeader>

      <div className="flex flex-wrap items-end gap-3">
        <div className="min-w-[220px] flex-1"><EntitySearch value={search} onChange={setSearch} placeholder="Buscar por código…" /></div>
        <div className="w-44">
          <Combobox id="ped-estado" label="Estado" variant="field" value={estado} onChange={(v) => { setEstado((v as EstadoPedido) ?? null); setPageNumber(1); }}
            options={ESTADO_PEDIDO.map((e) => ({ value: e.value, label: e.label }))} clearable emptyOptionLabel="Todos" placeholder="Todos" />
        </div>
      </div>

      {query.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 ? (
        <EntityEmpty icon={searchActive ? Search : ShoppingCart} title={searchActive ? "Sin resultados" : "Aún no hay pedidos"}
          body={searchActive ? `Nada coincide con "${debouncedSearch}".` : "Crea tu primer pedido para llevar el control de compras."}
          action={<Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]"><Plus className="mr-1.5 size-4" />Nuevo pedido</Button>} />
      ) : (
        <EntityListCard className="hidden md:block">
          <EntityListHeader className={COLS}>
            <span>Pedido</span><span>Tipo</span><span>Estado</span><span className="text-right">Costo</span><span className="text-right">Acciones</span>
          </EntityListHeader>
          {items.map((p, i) => (
            <EntityListRow key={p.id} className={COLS} isLast={i === items.length - 1} onClick={() => setEditor({ mode: "edit", pedido: p })}>
              <div className="min-w-0">
                <div className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{p.codigo}</div>
                <div className="text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(p.fechaPedido)}</div>
              </div>
              <div className="text-[13px] text-[var(--color-muted-foreground)]">{TIPO_PEDIDO.find((t) => t.value === p.tipo)?.label ?? p.tipo}</div>
              <div className={`text-[12.5px] font-medium ${ESTADO_TONE[p.estado]}`}>{p.estado}</div>
              <div className="text-right text-[13px] tabular-nums text-[var(--color-foreground)]">{money(p.costoReal ?? p.costoEstimado)}</div>
              <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                {p.estado === "Borrador" && (
                  <button type="button" title="Enviar" onClick={() => cambiar.mutate({ id: p.id, accion: "Enviar" })}
                    className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)]"><Send className="size-3.5" /></button>
                )}
                {p.estado === "Enviado" && (
                  <button type="button" title="Recibir" onClick={() => cambiar.mutate({ id: p.id, accion: "Recibir" })}
                    className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)]"><Check className="size-3.5" /></button>
                )}
                {(p.estado === "Borrador" || p.estado === "Enviado") && (
                  <button type="button" title="Cancelar" onClick={() => cambiar.mutate({ id: p.id, accion: "Cancelar" })}
                    className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"><X className="size-3.5" /></button>
                )}
                <button type="button" title="Editar" onClick={() => setEditor({ mode: "edit", pedido: p })}
                  className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)]"><Pencil className="size-3.5" /></button>
                <button type="button" title="Borrar" onClick={() => setEditor({ mode: "delete", pedido: p })}
                  className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)]"><Trash2 className="size-3.5" /></button>
              </div>
            </EntityListRow>
          ))}
        </EntityListCard>
      )}

      {data && items.length > 0 && (
        <EntityPager page={data.pageNumber} totalPages={data.totalPages} hasPrev={!!data.hasPrevious} hasNext={!!data.hasNext}
          onPrev={() => setPageNumber((p) => Math.max(1, p - 1))} onNext={() => setPageNumber((p) => p + 1)} />
      )}

      {query.isError && (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">{describe(query.error)}</div>
      )}

      <PedidoEditorDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <DeletePedidoDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function PedidoEditorDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const pedido = state.mode === "edit" ? state.pedido : undefined;
  const queryClient = useQueryClient();

  const proveedoresQ = useQuery({ queryKey: ["administration", "suppliers", "options"], queryFn: () => suppliersApi.search({ pageSize: 200, sortBy: "name", sortDir: "asc" }), enabled: isOpen });
  const lotesQ = useQuery({ queryKey: ["avicola", "lotes", "options"], queryFn: () => searchLotes({ pageSize: 200, sortBy: "fechaIngreso", sortDir: "desc" }), enabled: isOpen });
  const galponesQ = useQuery({ queryKey: ["avicola", "galpones", "options"], queryFn: () => searchGalpones({ pageSize: 200, sortBy: "nombre", sortDir: "asc" }), enabled: isOpen });

  const provOpts: ComboboxOption[] = (proveedoresQ.data?.items ?? []).map((s) => ({ value: s.id, label: s.name, hint: s.code ?? undefined }));
  const loteOpts: ComboboxOption[] = (lotesQ.data?.items ?? []).map((l) => ({ value: l.id, label: l.codigo }));
  const galponOpts: ComboboxOption[] = (galponesQ.data?.items ?? []).map((g) => ({ value: g.id, label: g.nombre }));

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);
  const initial = useMemo(() => ({
    codigo: pedido?.codigo ?? "",
    tipo: (pedido?.tipo ?? "Pienso") as TipoPedido,
    proveedorId: pedido?.proveedorId ?? null,
    descripcion: pedido?.descripcion ?? "",
    cantidad: pedido?.cantidad?.toString() ?? "",
    unidad: pedido?.unidad ?? "",
    costoEstimado: pedido?.costoEstimado?.toString() ?? "",
    estado: (pedido?.estado ?? "Borrador") as EstadoPedido,
    fechaPedido: isoToDateInput(pedido?.fechaPedido) || today,
    loteId: pedido?.loteId ?? null,
    galponId: pedido?.galponId ?? null,
    notas: pedido?.notas ?? "",
  }), [pedido, today]);

  const [f, setF] = useState(initial);
  useEffect(() => { if (isOpen) setF(initial); }, [isOpen, initial]);
  const set = <K extends keyof typeof initial>(k: K, v: (typeof initial)[K]) => setF((s) => ({ ...s, [k]: v }));

  const save = useMutation({
    mutationFn: (input: PedidoInput) => (pedido ? updatePedido(pedido.id, input) : createPedido(input)),
    onSuccess: () => { toast.success(pedido ? "Pedido actualizado" : "Pedido creado"); queryClient.invalidateQueries({ queryKey: ["avicola", "pedidos"] }); onClose(); },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const trimmed = f.codigo.trim();
  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trimmed) return;
    const base: PedidoInput = {
      codigo: trimmed, tipo: f.tipo, proveedorId: f.proveedorId, descripcion: f.descripcion.trim() || null,
      cantidad: toNum(f.cantidad) ?? 0, unidad: f.unidad.trim() || null, costoEstimado: toNum(f.costoEstimado),
      fechaPedido: dateToIso(f.fechaPedido) ?? new Date().toISOString(), loteId: f.loteId, galponId: f.galponId, notas: f.notas.trim() || null,
    };
    save.mutate(pedido ? base : { ...base, estado: f.estado });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-xl">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{pedido ? "Editar pedido" : "Nuevo pedido"}</DialogTitle>
            <DialogDescription>{pedido ? `Actualiza el pedido ${pedido.codigo}.` : "Registra un pedido a proveedor."}</DialogDescription>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <Field id="pe-codigo" label="Código" required><Input id="pe-codigo" value={f.codigo} onChange={(e) => set("codigo", e.target.value)} placeholder="P-2026-01" autoFocus required /></Field>
              <Field id="pe-tipo" label="Tipo"><Combobox id="pe-tipo" label="Tipo" variant="field" value={f.tipo} onChange={(v) => set("tipo", (v ?? "Pienso") as TipoPedido)} options={TIPO_PEDIDO.map((t) => ({ value: t.value, label: t.label }))} /></Field>
              <Field id="pe-prov" label="Proveedor"><Combobox id="pe-prov" label="Proveedor" variant="field" value={f.proveedorId} onChange={(v) => set("proveedorId", v)} options={provOpts} searchable clearable placeholder={proveedoresQ.isLoading ? "Cargando…" : "Sin asignar"} emptyOptionLabel="Sin asignar" /></Field>
              <Field id="pe-fecha" label="Fecha"><Input id="pe-fecha" type="date" value={f.fechaPedido} onChange={(e) => set("fechaPedido", e.target.value)} /></Field>
              <Field id="pe-cant" label="Cantidad"><Input id="pe-cant" type="number" step="any" value={f.cantidad} onChange={(e) => set("cantidad", e.target.value)} placeholder="0" /></Field>
              <Field id="pe-unidad" label="Unidad"><Input id="pe-unidad" value={f.unidad} onChange={(e) => set("unidad", e.target.value)} placeholder="kg / sacos / dosis" /></Field>
              <Field id="pe-costo" label="Costo estimado"><Input id="pe-costo" type="number" step="any" value={f.costoEstimado} onChange={(e) => set("costoEstimado", e.target.value)} placeholder="0" /></Field>
              {!pedido && (
                <Field id="pe-estado" label="Estado"><Combobox id="pe-estado" label="Estado" variant="field" value={f.estado} onChange={(v) => set("estado", (v ?? "Borrador") as EstadoPedido)} options={ESTADO_PEDIDO.map((e) => ({ value: e.value, label: e.label }))} /></Field>
              )}
              <Field id="pe-lote" label="Lote"><Combobox id="pe-lote" label="Lote" variant="field" value={f.loteId} onChange={(v) => set("loteId", v)} options={loteOpts} searchable clearable placeholder="Sin asignar" emptyOptionLabel="Sin asignar" /></Field>
              <Field id="pe-galpon" label="Galpón"><Combobox id="pe-galpon" label="Galpón" variant="field" value={f.galponId} onChange={(v) => set("galponId", v)} options={galponOpts} searchable clearable placeholder="Sin asignar" emptyOptionLabel="Sin asignar" /></Field>
            </div>
            <Field id="pe-desc" label="Descripción"><Input id="pe-desc" value={f.descripcion} onChange={(e) => set("descripcion", e.target.value)} placeholder="Opcional" /></Field>
            <Field id="pe-notas" label="Notas"><Input id="pe-notas" value={f.notas} onChange={(e) => set("notas", e.target.value)} placeholder="Opcional" /></Field>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild><Button type="button" variant="outline" disabled={save.isPending}>Cancelar</Button></DialogClose>
            <Button type="submit" disabled={save.isPending || !trimmed}>{save.isPending ? "Guardando…" : pedido ? "Guardar" : "Crear"}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function DeletePedidoDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const pedido = state.mode === "delete" ? state.pedido : undefined;
  const queryClient = useQueryClient();
  const del = useMutation({
    mutationFn: (id: string) => deletePedido(id),
    onSuccess: () => { toast.success("Pedido borrado"); queryClient.invalidateQueries({ queryKey: ["avicola", "pedidos"] }); onClose(); },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });
  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar pedido</DialogTitle>
          <DialogDescription>Esto elimina permanentemente el pedido <span className="font-medium text-[var(--color-foreground)]">{pedido?.codigo}</span>.</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild><Button type="button" variant="outline" disabled={del.isPending}>Cancelar</Button></DialogClose>
          <Button variant="destructive" onClick={() => pedido && del.mutate(pedido.id)} disabled={del.isPending || !pedido}>{del.isPending ? "Borrando…" : "Borrar"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
