import { useEffect, useMemo, useState, type FormEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, Search, Trash2, Warehouse } from "lucide-react";
import { toast } from "sonner";
import {
  createGalpon,
  deleteGalpon,
  searchGalpones,
  updateGalpon,
  type GalponDto,
  type GalponInput,
} from "@/api/avicola";
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
import { describe } from "@/lib/list-helpers";

const PAGE_SIZE = 20;
const COLS = "grid-cols-[1fr_120px_120px_48px]";

function num(n: number | null | undefined): string {
  if (n === null || n === undefined) return "—";
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(n);
}

function toNum(s: string): number | null {
  const t = s.trim();
  if (t === "") return null;
  const n = Number(t);
  return Number.isFinite(n) ? n : null;
}

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; galpon: GalponDto }
  | { mode: "delete"; galpon: GalponDto };

export function GalponesPage() {
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
    queryKey: ["avicola", "galpones", "list", { search: debouncedSearch, pageNumber }],
    queryFn: () =>
      searchGalpones({
        search: debouncedSearch || undefined,
        pageNumber,
        pageSize: PAGE_SIZE,
        sortBy: "nombre",
        sortDir: "asc",
      }),
    placeholderData: keepPreviousData,
  });

  const data = query.data;
  const items = data?.items ?? [];
  const searchActive = debouncedSearch.length > 0;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Warehouse}
        title="Galpones"
        total={data?.totalCount ?? null}
        unit="galpón"
        description="Las casetas donde se crían los lotes: capacidad, superficie y ubicación."
      >
        <Button
          onClick={() => setEditor({ mode: "create" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Plus className="size-4" />
          Nuevo galpón
        </Button>
      </EntityPageHeader>

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por nombre o código…" />

      {query.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 ? (
        <EntityEmpty
          icon={searchActive ? Search : Warehouse}
          title={searchActive ? "Sin resultados" : "Aún no hay galpones"}
          body={
            searchActive
              ? `Nada coincide con "${debouncedSearch}".`
              : "Crea tu primer galpón para empezar a asignar lotes."
          }
          action={
            <Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]">
              <Plus className="mr-1.5 size-4" />
              Nuevo galpón
            </Button>
          }
        />
      ) : (
        <div>
          <div className="space-y-2 md:hidden">
            {items.map((g) => (
              <EntityMobileCard key={g.id} onClick={() => setEditor({ mode: "edit", galpon: g })}>
                <div className="flex items-center justify-between">
                  <div className="flex min-w-0 items-center gap-3">
                    <EntityInitialsAvatar name={g.nombre} size={40} />
                    <div className="min-w-0">
                      <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{g.nombre}</p>
                      <p className="mt-0.5 text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">
                        Capacidad {num(g.capacidad)} · {g.activo ? "Activo" : "Inactivo"}
                      </p>
                    </div>
                  </div>
                </div>
              </EntityMobileCard>
            ))}
          </div>

          <EntityListCard className="hidden md:block">
            <EntityListHeader className={COLS}>
              <span>Galpón</span>
              <span className="text-right">Capacidad</span>
              <span className="text-right">Estado</span>
              <span />
            </EntityListHeader>

            {items.map((g, i) => (
              <EntityListRow key={g.id} className={COLS} isLast={i === items.length - 1} onClick={() => setEditor({ mode: "edit", galpon: g })}>
                <div className="flex min-w-0 items-center gap-3">
                  <EntityInitialsAvatar name={g.nombre} size={36} />
                  <div className="min-w-0">
                    <div className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{g.nombre}</div>
                    {g.codigo && <div className="truncate text-[11.5px] text-[var(--color-muted-foreground)]">{g.codigo}</div>}
                  </div>
                </div>
                <div className="text-right text-[13px] text-[var(--color-foreground)] tabular-nums">{num(g.capacidad)}</div>
                <div className="text-right text-[13px] tabular-nums">
                  <span className={g.activo ? "text-[var(--color-primary)]" : "text-[var(--color-muted-foreground)]"}>
                    {g.activo ? "Activo" : "Inactivo"}
                  </span>
                </div>
                <div className="flex items-center justify-end gap-1">
                  <button
                    type="button"
                    aria-label={`Editar ${g.nombre}`}
                    onClick={(e) => { e.stopPropagation(); setEditor({ mode: "edit", galpon: g }); }}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"
                  >
                    <Pencil className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Borrar ${g.nombre}`}
                    onClick={(e) => { e.stopPropagation(); setEditor({ mode: "delete", galpon: g }); }}
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

      <GalponEditorDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <DeleteGalponDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function GalponEditorDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const galpon = state.mode === "edit" ? state.galpon : undefined;
  const queryClient = useQueryClient();

  const initial = useMemo(
    () => ({
      nombre: galpon?.nombre ?? "",
      codigo: galpon?.codigo ?? "",
      capacidad: galpon?.capacidad?.toString() ?? "",
      superficieM2: galpon?.superficieM2?.toString() ?? "",
      ubicacion: galpon?.ubicacion ?? "",
      activo: galpon?.activo ?? true,
      notas: galpon?.notas ?? "",
    }),
    [galpon],
  );

  const [nombre, setNombre] = useState(initial.nombre);
  const [codigo, setCodigo] = useState(initial.codigo);
  const [capacidad, setCapacidad] = useState(initial.capacidad);
  const [superficieM2, setSuperficieM2] = useState(initial.superficieM2);
  const [ubicacion, setUbicacion] = useState(initial.ubicacion);
  const [activo, setActivo] = useState(initial.activo);
  const [notas, setNotas] = useState(initial.notas);

  useEffect(() => {
    if (isOpen) {
      setNombre(initial.nombre);
      setCodigo(initial.codigo);
      setCapacidad(initial.capacidad);
      setSuperficieM2(initial.superficieM2);
      setUbicacion(initial.ubicacion);
      setActivo(initial.activo);
      setNotas(initial.notas);
    }
  }, [isOpen, initial]);

  const save = useMutation({
    mutationFn: (input: GalponInput) => (galpon ? updateGalpon(galpon.id, input) : createGalpon(input)),
    onSuccess: () => {
      toast.success(galpon ? "Galpón actualizado" : "Galpón creado");
      queryClient.invalidateQueries({ queryKey: ["avicola", "galpones"] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const trimmed = nombre.trim();

  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trimmed) return;
    save.mutate({
      nombre: trimmed,
      codigo: codigo.trim() || null,
      capacidad: toNum(capacidad) ?? 0,
      superficieM2: toNum(superficieM2),
      ubicacion: ubicacion.trim() || null,
      activo,
      notas: notas.trim() || null,
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-lg">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{galpon ? "Editar galpón" : "Nuevo galpón"}</DialogTitle>
            <DialogDescription>
              {galpon ? `Actualiza los datos de ${galpon.nombre}.` : "Crea un galpón para alojar lotes."}
            </DialogDescription>
          </DialogHeader>

          <DialogBody className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <Field id="g-nombre" label="Nombre" required>
                <Input id="g-nombre" value={nombre} onChange={(e) => setNombre(e.target.value)} placeholder="Galpón 1" autoFocus required />
              </Field>
              <Field id="g-codigo" label="Código">
                <Input id="g-codigo" value={codigo} onChange={(e) => setCodigo(e.target.value)} placeholder="G-01" />
              </Field>
              <Field id="g-cap" label="Capacidad (aves)">
                <Input id="g-cap" type="number" step="1" value={capacidad} onChange={(e) => setCapacidad(e.target.value)} placeholder="0" />
              </Field>
              <Field id="g-sup" label="Superficie (m²)">
                <Input id="g-sup" type="number" step="any" value={superficieM2} onChange={(e) => setSuperficieM2(e.target.value)} placeholder="0" />
              </Field>
              <Field id="g-ubi" label="Ubicación">
                <Input id="g-ubi" value={ubicacion} onChange={(e) => setUbicacion(e.target.value)} placeholder="Sector norte" />
              </Field>
              <Field id="g-notas" label="Notas">
                <Input id="g-notas" value={notas} onChange={(e) => setNotas(e.target.value)} placeholder="Opcional" />
              </Field>
            </div>
            <label className="flex cursor-pointer items-center gap-2 text-[13px] text-[var(--color-foreground)]">
              <input type="checkbox" checked={activo} onChange={(e) => setActivo(e.target.checked)} className="size-4 accent-[var(--color-primary)]" />
              Galpón activo
            </label>
          </DialogBody>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={save.isPending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={save.isPending || !trimmed}>
              {save.isPending ? "Guardando…" : galpon ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function DeleteGalponDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const galpon = state.mode === "delete" ? state.galpon : undefined;
  const queryClient = useQueryClient();

  const del = useMutation({
    mutationFn: (id: string) => deleteGalpon(id),
    onSuccess: () => {
      toast.success("Galpón borrado");
      queryClient.invalidateQueries({ queryKey: ["avicola", "galpones"] });
      onClose();
    },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar galpón</DialogTitle>
          <DialogDescription>
            Esto elimina permanentemente{" "}
            <span className="font-medium text-[var(--color-foreground)]">{galpon?.nombre}</span>. Los lotes que lo referencian quedarán sin galpón.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={del.isPending}>Cancelar</Button>
          </DialogClose>
          <Button variant="destructive" onClick={() => galpon && del.mutate(galpon.id)} disabled={del.isPending || !galpon}>
            {del.isPending ? "Borrando…" : "Borrar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
