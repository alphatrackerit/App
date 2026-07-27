import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Bird, ChevronRight, Pencil, Plus, Search, Trash2 } from "lucide-react";
import { toast } from "sonner";
import {
  createLote,
  deleteLote,
  searchGalpones,
  searchLotes,
  updateLote,
  ESTADO_LOTE,
  type EstadoLote,
  type LoteDto,
  type LoteInput,
} from "@/api/avicola";
import { suppliersApi } from "@/api/administration";
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
  type ComboboxOption,
  EntityEmpty,
  EntityInitialsAvatar,
  EntityListCard,
  EntityListHeader,
  EntityListLoading,
  EntityListRow,
  EntityMobileCard,
  EntityPageHeader,
  EntityPager,
  EntitySearch,
  Field,
} from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

const PAGE_SIZE = 20;
const COLS = "grid-cols-[1fr_140px_120px_120px_48px]";

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

const ESTADO_LABEL: Record<EstadoLote, string> = {
  Planificado: "Planificado",
  EnCrianza: "En crianza",
  Finalizado: "Finalizado",
  Cancelado: "Cancelado",
};

function EstadoBadge({ estado }: { estado: EstadoLote }) {
  const tone =
    estado === "EnCrianza"
      ? "text-[var(--color-primary)]"
      : estado === "Finalizado"
        ? "text-[var(--color-muted-foreground)]"
        : estado === "Cancelado"
          ? "text-[var(--color-destructive)]"
          : "text-[var(--color-foreground)]";
  return <span className={`text-[12.5px] font-medium ${tone}`}>{ESTADO_LABEL[estado]}</span>;
}

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; lote: LoteDto }
  | { mode: "delete"; lote: LoteDto };

export function LotesPage() {
  const navigate = useNavigate();
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [editor, setEditor] = useState<EditorState>({ mode: "closed" });

  useEffect(() => {
    const t = setTimeout(() => {
      setDebouncedSearch(search.trim());
      setPageNumber(1);
    }, 250);
    return () => clearTimeout(t);
  }, [search]);

  const query = useQuery({
    queryKey: ["avicola", "lotes", "list", { search: debouncedSearch, pageNumber }],
    queryFn: () =>
      searchLotes({
        search: debouncedSearch || undefined,
        pageNumber,
        pageSize: PAGE_SIZE,
        sortBy: "fechaIngreso",
        sortDir: "desc",
      }),
    placeholderData: keepPreviousData,
  });

  const data = query.data;
  const items = data?.items ?? [];
  const searchActive = debouncedSearch.length > 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Bird}
        title="Lotes"
        total={data?.totalCount ?? null}
        unit="lote"
        description="Cada lote de pollos de engorde: ingreso, raza, cantidad y estado de crianza."
      >
        <Button
          onClick={() => setEditor({ mode: "create" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Plus className="size-4" />
          Nuevo lote
        </Button>
      </EntityPageHeader>

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por código…" />

      {query.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 ? (
        <EntityEmpty
          icon={searchActive ? Search : Bird}
          title={searchActive ? "Sin resultados" : "Aún no hay lotes"}
          body={
            searchActive
              ? `Nada coincide con "${debouncedSearch}".`
              : "Crea tu primer lote para registrar mortalidad, alimentación, pesos, sanidad y despachos."
          }
          action={
            <Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]">
              <Plus className="mr-1.5 size-4" />
              Nuevo lote
            </Button>
          }
        />
      ) : (
        <div>
          <div className="space-y-2 md:hidden">
            {items.map((l) => (
              <EntityMobileCard
                key={l.id}
                href={`/avicola/lotes/${l.id}`}
                onClick={(e) => { e.preventDefault(); navigate(`/avicola/lotes/${l.id}`); }}
                aria-label={`Abrir ${l.codigo}`}
              >
                <div className="flex items-center justify-between">
                  <div className="flex min-w-0 items-center gap-3">
                    <EntityInitialsAvatar name={l.codigo} size={40} />
                    <div className="min-w-0">
                      <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{l.codigo}</p>
                      <p className="mt-0.5 text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">
                        {formatDate(l.fechaIngreso)} · {l.cantidadInicial} aves
                      </p>
                    </div>
                  </div>
                  <ChevronRight className="size-4 shrink-0 text-[var(--color-border)]" />
                </div>
              </EntityMobileCard>
            ))}
          </div>

          <EntityListCard className="hidden md:block">
            <EntityListHeader className={COLS}>
              <span>Lote</span>
              <span>Ingreso</span>
              <span className="text-right">Aves</span>
              <span>Estado</span>
              <span />
            </EntityListHeader>

            {items.map((l, i) => (
              <EntityListRow key={l.id} className={COLS} isLast={i === items.length - 1} onClick={() => navigate(`/avicola/lotes/${l.id}`)}>
                <div className="flex min-w-0 items-center gap-3">
                  <EntityInitialsAvatar name={l.codigo} size={36} />
                  <div className="min-w-0">
                    <div className="truncate text-[14px] font-medium text-[var(--color-foreground)] transition-colors group-hover:text-[var(--color-primary)]">{l.codigo}</div>
                    {l.raza && <div className="truncate text-[11.5px] text-[var(--color-muted-foreground)]">{l.raza}</div>}
                  </div>
                </div>
                <div className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(l.fechaIngreso)}</div>
                <div className="text-right text-[13px] text-[var(--color-foreground)] tabular-nums">{l.cantidadInicial}</div>
                <div><EstadoBadge estado={l.estado} /></div>
                <div className="flex items-center justify-end gap-1">
                  <button
                    type="button"
                    aria-label={`Editar ${l.codigo}`}
                    onClick={(e) => { e.stopPropagation(); setEditor({ mode: "edit", lote: l }); }}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"
                  >
                    <Pencil className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Borrar ${l.codigo}`}
                    onClick={(e) => { e.stopPropagation(); setEditor({ mode: "delete", lote: l }); }}
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

      {query.isError && (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(query.error)}
        </div>
      )}

      <LoteEditorDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <DeleteLoteDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function LoteEditorDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const lote = state.mode === "edit" ? state.lote : undefined;
  const queryClient = useQueryClient();

  const galponesQ = useQuery({
    queryKey: ["avicola", "galpones", "options"],
    queryFn: () => searchGalpones({ pageSize: 10000, sortBy: "nombre", sortDir: "asc" }),
    enabled: isOpen,
  });
  const proveedoresQ = useQuery({
    queryKey: ["administration", "suppliers", "options"],
    queryFn: () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    enabled: isOpen,
  });

  const galponOptions: ComboboxOption[] = (galponesQ.data?.items ?? []).map((g) => ({ value: g.id, label: g.nombre, hint: g.codigo ?? undefined }));
  const proveedorOptions: ComboboxOption[] = (proveedoresQ.data?.items ?? []).map((s) => ({ value: s.id, label: s.name, hint: s.code ?? undefined }));

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);
  const initial = useMemo(
    () => ({
      codigo: lote?.codigo ?? "",
      galponId: lote?.galponId ?? null,
      raza: lote?.raza ?? "",
      fechaIngreso: isoToDateInput(lote?.fechaIngreso) || today,
      cantidadInicial: lote?.cantidadInicial?.toString() ?? "",
      pesoInicialGramos: lote?.pesoInicialGramos?.toString() ?? "",
      fechaSalidaPrevista: isoToDateInput(lote?.fechaSalidaPrevista),
      estado: (lote?.estado ?? "EnCrianza") as EstadoLote,
      proveedorId: lote?.proveedorId ?? null,
      costoPolluelo: lote?.costoPolluelo?.toString() ?? "",
      notas: lote?.notas ?? "",
    }),
    [lote, today],
  );

  const [codigo, setCodigo] = useState(initial.codigo);
  const [galponId, setGalponId] = useState<string | null>(initial.galponId);
  const [raza, setRaza] = useState(initial.raza);
  const [fechaIngreso, setFechaIngreso] = useState(initial.fechaIngreso);
  const [cantidadInicial, setCantidadInicial] = useState(initial.cantidadInicial);
  const [pesoInicialGramos, setPesoInicialGramos] = useState(initial.pesoInicialGramos);
  const [fechaSalidaPrevista, setFechaSalidaPrevista] = useState(initial.fechaSalidaPrevista);
  const [estado, setEstado] = useState<EstadoLote>(initial.estado);
  const [proveedorId, setProveedorId] = useState<string | null>(initial.proveedorId);
  const [costoPolluelo, setCostoPolluelo] = useState(initial.costoPolluelo);
  const [notas, setNotas] = useState(initial.notas);

  useEffect(() => {
    if (isOpen) {
      setCodigo(initial.codigo);
      setGalponId(initial.galponId);
      setRaza(initial.raza);
      setFechaIngreso(initial.fechaIngreso);
      setCantidadInicial(initial.cantidadInicial);
      setPesoInicialGramos(initial.pesoInicialGramos);
      setFechaSalidaPrevista(initial.fechaSalidaPrevista);
      setEstado(initial.estado);
      setProveedorId(initial.proveedorId);
      setCostoPolluelo(initial.costoPolluelo);
      setNotas(initial.notas);
    }
  }, [isOpen, initial]);

  const save = useMutation({
    mutationFn: (input: LoteInput) => (lote ? updateLote(lote.id, input) : createLote(input)),
    onSuccess: () => {
      toast.success(lote ? "Lote actualizado" : "Lote creado");
      queryClient.invalidateQueries({ queryKey: ["avicola", "lotes"] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const trimmed = codigo.trim();

  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trimmed) return;
    save.mutate({
      codigo: trimmed,
      galponId,
      raza: raza.trim() || null,
      fechaIngreso: dateToIso(fechaIngreso) ?? new Date().toISOString(),
      cantidadInicial: toNum(cantidadInicial) ?? 0,
      pesoInicialGramos: toNum(pesoInicialGramos),
      fechaSalidaPrevista: dateToIso(fechaSalidaPrevista),
      estado,
      proveedorId,
      costoPolluelo: toNum(costoPolluelo),
      notas: notas.trim() || null,
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-xl">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{lote ? "Editar lote" : "Nuevo lote"}</DialogTitle>
            <DialogDescription>
              {lote ? `Actualiza los datos del lote ${lote.codigo}.` : "Registra el ingreso de un nuevo lote de pollitos."}
            </DialogDescription>
          </DialogHeader>

          <DialogBody className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <Field id="l-codigo" label="Código" required>
                <Input id="l-codigo" value={codigo} onChange={(e) => setCodigo(e.target.value)} placeholder="L-2026-01" autoFocus required />
              </Field>
              <Field id="l-raza" label="Raza">
                <Input id="l-raza" value={raza} onChange={(e) => setRaza(e.target.value)} placeholder="Ross 308" />
              </Field>
              <Field id="l-galpon" label="Galpón">
                <Combobox
                  id="l-galpon"
                  label="Galpón"
                  variant="field"
                  value={galponId}
                  onChange={setGalponId}
                  options={galponOptions}
                  searchable
                  clearable
                  placeholder={galponesQ.isLoading ? "Cargando…" : "Sin asignar"}
                  emptyOptionLabel="Sin asignar"
                />
              </Field>
              <Field id="l-estado" label="Estado">
                <Combobox
                  id="l-estado"
                  label="Estado"
                  variant="field"
                  value={estado}
                  onChange={(v) => setEstado((v ?? "EnCrianza") as EstadoLote)}
                  options={ESTADO_LOTE.map((e) => ({ value: e.value, label: e.label }))}
                />
              </Field>
              <Field id="l-ingreso" label="Fecha de ingreso" required>
                <Input id="l-ingreso" type="date" value={fechaIngreso} onChange={(e) => setFechaIngreso(e.target.value)} required />
              </Field>
              <Field id="l-salida" label="Salida prevista">
                <Input id="l-salida" type="date" value={fechaSalidaPrevista} onChange={(e) => setFechaSalidaPrevista(e.target.value)} />
              </Field>
              <Field id="l-cant" label="Cantidad inicial (aves)">
                <Input id="l-cant" type="number" step="1" value={cantidadInicial} onChange={(e) => setCantidadInicial(e.target.value)} placeholder="0" />
              </Field>
              <Field id="l-peso" label="Peso inicial (g)">
                <Input id="l-peso" type="number" step="any" value={pesoInicialGramos} onChange={(e) => setPesoInicialGramos(e.target.value)} placeholder="42" />
              </Field>
              <Field id="l-prov" label="Proveedor">
                <Combobox
                  id="l-prov"
                  label="Proveedor"
                  variant="field"
                  value={proveedorId}
                  onChange={setProveedorId}
                  options={proveedorOptions}
                  searchable
                  clearable
                  placeholder={proveedoresQ.isLoading ? "Cargando…" : "Sin asignar"}
                  emptyOptionLabel="Sin asignar"
                />
              </Field>
              <Field id="l-costo" label="Costo por pollito">
                <Input id="l-costo" type="number" step="any" value={costoPolluelo} onChange={(e) => setCostoPolluelo(e.target.value)} placeholder="0" />
              </Field>
            </div>
            <Field id="l-notas" label="Notas">
              <Input id="l-notas" value={notas} onChange={(e) => setNotas(e.target.value)} placeholder="Opcional" />
            </Field>
          </DialogBody>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={save.isPending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={save.isPending || !trimmed}>
              {save.isPending ? "Guardando…" : lote ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function DeleteLoteDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const lote = state.mode === "delete" ? state.lote : undefined;
  const queryClient = useQueryClient();

  const del = useMutation({
    mutationFn: (id: string) => deleteLote(id),
    onSuccess: () => {
      toast.success("Lote borrado");
      queryClient.invalidateQueries({ queryKey: ["avicola", "lotes"] });
      onClose();
    },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar lote</DialogTitle>
          <DialogDescription>
            Esto elimina permanentemente el lote{" "}
            <span className="font-medium text-[var(--color-foreground)]">{lote?.codigo}</span>. Sus registros (mortalidad, alimentación, pesos, sanidad, despachos) deben borrarse antes.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={del.isPending}>Cancelar</Button>
          </DialogClose>
          <Button variant="destructive" onClick={() => lote && del.mutate(lote.id)} disabled={del.isPending || !lote}>
            {del.isPending ? "Borrando…" : "Borrar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
