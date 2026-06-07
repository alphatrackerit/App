import { useEffect, useMemo, useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Briefcase, ChevronRight, Pencil, Plus, Search, Trash2 } from "lucide-react";
import { toast } from "sonner";
import {
  createProject,
  deleteProject,
  searchProjects,
  updateProject,
  type ProjectDto,
  type ProjectInput,
} from "@/api/projects";
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
const COLS = "grid-cols-[1fr_120px_120px_120px_48px]";

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

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; project: ProjectDto }
  | { mode: "delete"; project: ProjectDto };

export function ProjectsPage() {
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
    queryKey: ["projects", "list", { search: debouncedSearch, pageNumber, pageSize: PAGE_SIZE }],
    queryFn: () =>
      searchProjects({
        search: debouncedSearch || undefined,
        pageNumber,
        pageSize: PAGE_SIZE,
        sortBy: "name",
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
        icon={Briefcase}
        title="Proyectos"
        total={data?.totalCount ?? null}
        unit="proyecto"
        description="Gestiona los proyectos y su rentabilidad: venta, coste y beneficio, con sus ingresos, pagos y notas."
      >
        <Button
          onClick={() => setEditor({ mode: "create" })}
          className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none"
        >
          <Plus className="size-4" />
          Nuevo proyecto
        </Button>
      </EntityPageHeader>

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por nombre…" />

      {query.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 ? (
        <EntityEmpty
          icon={searchActive ? Search : Briefcase}
          title={searchActive ? "Sin resultados" : "Aún no hay proyectos"}
          body={
            searchActive
              ? `Nada coincide con "${debouncedSearch}". Prueba otro término o limpia la búsqueda.`
              : "Crea tu primer proyecto para empezar a registrar ingresos, pagos y notas."
          }
          action={
            searchActive ? (
              <Button variant="outline" onClick={() => setSearch("")} className="h-9 rounded-lg px-4 text-[13px]">
                Limpiar búsqueda
              </Button>
            ) : (
              <Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]">
                <Plus className="mr-1.5 size-4" />
                Nuevo proyecto
              </Button>
            )
          }
        />
      ) : (
        <div>
          {/* Mobile */}
          <div className="space-y-2 md:hidden">
            {items.map((p) => (
              <EntityMobileCard
                key={p.id}
                href={`/projects/${p.id}`}
                onClick={(e) => {
                  e.preventDefault();
                  navigate(`/projects/${p.id}`);
                }}
                aria-label={`Abrir ${p.name}`}
              >
                <div className="flex items-center justify-between">
                  <div className="flex min-w-0 items-center gap-3">
                    <EntityInitialsAvatar name={p.name} size={40} />
                    <div className="min-w-0">
                      <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{p.name}</p>
                      <p className="mt-0.5 text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">
                        Venta {money(p.salePrice)} · Beneficio {money(p.profit)}
                      </p>
                    </div>
                  </div>
                  <ChevronRight className="size-4 shrink-0 text-[var(--color-border)]" />
                </div>
              </EntityMobileCard>
            ))}
          </div>

          {/* Desktop */}
          <EntityListCard className="hidden md:block">
            <EntityListHeader className={COLS}>
              <span>Proyecto</span>
              <span className="text-right">Venta</span>
              <span className="text-right">Coste</span>
              <span className="text-right">Beneficio</span>
              <span />
            </EntityListHeader>

            {items.map((p, i) => (
              <EntityListRow key={p.id} className={COLS} isLast={i === items.length - 1} onClick={() => navigate(`/projects/${p.id}`)}>
                <div className="flex min-w-0 items-center gap-3">
                  <EntityInitialsAvatar name={p.name} size={36} />
                  <div className="truncate text-[14px] font-medium text-[var(--color-foreground)] transition-colors group-hover:text-[var(--color-primary)]">
                    {p.name}
                  </div>
                </div>
                <div className="text-right text-[13px] text-[var(--color-foreground)] tabular-nums">{money(p.salePrice)}</div>
                <div className="text-right text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{money(p.cost)}</div>
                <div className="text-right text-[13px] font-medium text-[var(--color-foreground)] tabular-nums">{money(p.profit)}</div>
                <div className="flex items-center justify-end gap-1">
                  <button
                    type="button"
                    aria-label={`Editar ${p.name}`}
                    onClick={(e) => {
                      e.stopPropagation();
                      setEditor({ mode: "edit", project: p });
                    }}
                    className="grid size-7 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"
                  >
                    <Pencil className="size-3.5" />
                  </button>
                  <button
                    type="button"
                    aria-label={`Borrar ${p.name}`}
                    onClick={(e) => {
                      e.stopPropagation();
                      setEditor({ mode: "delete", project: p });
                    }}
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
        <div
          role="alert"
          className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]"
        >
          {describe(query.error)}
        </div>
      )}

      <ProjectEditorDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <DeleteProjectDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function ProjectEditorDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const project = state.mode === "edit" ? state.project : undefined;
  const queryClient = useQueryClient();

  const initial = useMemo(
    () => ({
      name: project?.name ?? "",
      salePrice: project?.salePrice?.toString() ?? "",
      forecastSale: project?.forecastSale?.toString() ?? "",
      cost: project?.cost?.toString() ?? "",
      forecastCost: project?.forecastCost?.toString() ?? "",
      profit: project?.profit?.toString() ?? "",
    }),
    [project],
  );

  const [name, setName] = useState(initial.name);
  const [salePrice, setSalePrice] = useState(initial.salePrice);
  const [forecastSale, setForecastSale] = useState(initial.forecastSale);
  const [cost, setCost] = useState(initial.cost);
  const [forecastCost, setForecastCost] = useState(initial.forecastCost);
  const [profit, setProfit] = useState(initial.profit);

  useEffect(() => {
    if (isOpen) {
      setName(initial.name);
      setSalePrice(initial.salePrice);
      setForecastSale(initial.forecastSale);
      setCost(initial.cost);
      setForecastCost(initial.forecastCost);
      setProfit(initial.profit);
    }
  }, [isOpen, initial]);

  const save = useMutation({
    mutationFn: (input: ProjectInput) =>
      project ? updateProject(project.id, input) : createProject(input),
    onSuccess: () => {
      toast.success(project ? "Proyecto actualizado" : "Proyecto creado");
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const trimmedName = name.trim();

  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trimmedName) return;
    save.mutate({
      name: trimmedName,
      salePrice: toNum(salePrice),
      forecastSale: toNum(forecastSale),
      cost: toNum(cost),
      forecastCost: toNum(forecastCost),
      profit: toNum(profit),
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-lg">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{project ? "Editar proyecto" : "Nuevo proyecto"}</DialogTitle>
            <DialogDescription>
              {project
                ? `Actualiza los datos de ${project.name}.`
                : "Crea un proyecto. Los importes son opcionales."}
            </DialogDescription>
          </DialogHeader>

          <DialogBody className="space-y-5">
            <Field id="p-name" label="Nombre" required>
              <Input id="p-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Obra Centro" autoFocus required />
            </Field>

            <div className="grid grid-cols-2 gap-4">
              <Field id="p-sale" label="Precio venta">
                <Input id="p-sale" type="number" step="any" value={salePrice} onChange={(e) => setSalePrice(e.target.value)} placeholder="0" />
              </Field>
              <Field id="p-fsale" label="Venta prevista">
                <Input id="p-fsale" type="number" step="any" value={forecastSale} onChange={(e) => setForecastSale(e.target.value)} placeholder="0" />
              </Field>
              <Field id="p-cost" label="Coste">
                <Input id="p-cost" type="number" step="any" value={cost} onChange={(e) => setCost(e.target.value)} placeholder="0" />
              </Field>
              <Field id="p-fcost" label="Coste previsto">
                <Input id="p-fcost" type="number" step="any" value={forecastCost} onChange={(e) => setForecastCost(e.target.value)} placeholder="0" />
              </Field>
              <Field id="p-profit" label="Beneficio">
                <Input id="p-profit" type="number" step="any" value={profit} onChange={(e) => setProfit(e.target.value)} placeholder="0" />
              </Field>
            </div>
          </DialogBody>

          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={save.isPending}>
                Cancelar
              </Button>
            </DialogClose>
            <Button type="submit" disabled={save.isPending || !trimmedName}>
              {save.isPending ? "Guardando…" : project ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function DeleteProjectDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const project = state.mode === "delete" ? state.project : undefined;
  const queryClient = useQueryClient();

  const del = useMutation({
    mutationFn: (id: string) => deleteProject(id),
    onSuccess: () => {
      toast.success("Proyecto borrado");
      queryClient.invalidateQueries({ queryKey: ["projects"] });
      onClose();
    },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar proyecto</DialogTitle>
          <DialogDescription>
            Esto elimina permanentemente{" "}
            <span className="font-medium text-[var(--color-foreground)]">{project?.name}</span>. Los ingresos/pagos/notas
            asociados deberán reasignarse o borrarse antes.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={del.isPending}>
              Cancelar
            </Button>
          </DialogClose>
          <Button variant="destructive" onClick={() => project && del.mutate(project.id)} disabled={del.isPending || !project}>
            {del.isPending ? "Borrando…" : "Borrar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
