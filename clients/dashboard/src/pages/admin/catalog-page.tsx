import { useEffect, useMemo, useState, type FormEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Pencil, Plus, Search, Trash2, type LucideIcon } from "lucide-react";
import { toast } from "sonner";
import type { CatalogApi, Lookup } from "@/api/administration";
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
const COLS = "grid-cols-[1fr_160px_48px]";

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; item: Lookup }
  | { mode: "delete"; item: Lookup };

export function CatalogPage({
  title,
  unit,
  icon,
  queryKey,
  api,
  description,
}: {
  title: string;
  unit: string;
  icon: LucideIcon;
  queryKey: string;
  api: CatalogApi;
  description?: string;
}) {
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

      <EntitySearch value={search} onChange={setSearch} placeholder="Buscar por nombre o código…" />

      {q.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 ? (
        <EntityEmpty
          icon={searchActive ? Search : icon}
          title={searchActive ? "Sin resultados" : "Aún no hay registros"}
          body={
            searchActive
              ? `Nada coincide con "${debouncedSearch}".`
              : "Crea el primer registro para empezar."
          }
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
                <div className="flex items-center gap-3">
                  <EntityInitialsAvatar name={it.name} size={36} />
                  <div className="min-w-0">
                    <p className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</p>
                    {it.code && (
                      <code className="font-mono text-[11px] text-[var(--color-muted-foreground)]">{it.code}</code>
                    )}
                  </div>
                </div>
              </EntityMobileCard>
            ))}
          </div>

          <EntityListCard className="hidden md:block">
            <EntityListHeader className={COLS}>
              <span>Nombre</span>
              <span>Código</span>
              <span />
            </EntityListHeader>
            {items.map((it, i) => (
              <EntityListRow key={it.id} className={COLS} isLast={i === items.length - 1}>
                <div className="flex min-w-0 items-center gap-3">
                  <EntityInitialsAvatar name={it.name} size={36} />
                  <span className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{it.name}</span>
                </div>
                <code className="truncate font-mono text-[12px] text-[var(--color-muted-foreground)]">{it.code ?? "—"}</code>
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

      <EditorDialog state={editor} title={title} queryKey={queryKey} api={api} onClose={() => setEditor({ mode: "closed" })} />
      <DeleteDialog state={editor} queryKey={queryKey} api={api} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function EditorDialog({
  state,
  title,
  queryKey,
  api,
  onClose,
}: {
  state: EditorState;
  title: string;
  queryKey: string;
  api: CatalogApi;
  onClose: () => void;
}) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const item = state.mode === "edit" ? state.item : undefined;
  const queryClient = useQueryClient();

  const initial = useMemo(() => ({ name: item?.name ?? "", code: item?.code ?? "" }), [item]);
  const [name, setName] = useState(initial.name);
  const [code, setCode] = useState(initial.code);

  useEffect(() => {
    if (isOpen) {
      setName(initial.name);
      setCode(initial.code);
    }
  }, [isOpen, initial]);

  const save = useMutation({
    mutationFn: () => {
      const input = { name: name.trim(), code: code.trim() || null };
      return item ? api.update(item.id, input) : api.create(input);
    },
    onSuccess: () => {
      toast.success(item ? "Actualizado" : "Creado");
      queryClient.invalidateQueries({ queryKey: ["administration", queryKey] });
      onClose();
    },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const trimmed = name.trim();
  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trimmed) return;
    save.mutate();
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-md">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{item ? `Editar ${title.toLowerCase()}` : `Nuevo en ${title}`}</DialogTitle>
            <DialogDescription>Nombre obligatorio. El código es opcional.</DialogDescription>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <Field id="c-name" label="Nombre" required>
              <Input id="c-name" value={name} onChange={(e) => setName(e.target.value)} placeholder="Nombre" autoFocus required maxLength={256} />
            </Field>
            <Field id="c-code" label="Código">
              <Input id="c-code" value={code} onChange={(e) => setCode(e.target.value)} placeholder="Opcional" maxLength={64} />
            </Field>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild>
              <Button type="button" variant="outline" disabled={save.isPending}>Cancelar</Button>
            </DialogClose>
            <Button type="submit" disabled={save.isPending || !trimmed}>
              {save.isPending ? "Guardando…" : item ? "Guardar" : "Crear"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function DeleteDialog({
  state,
  queryKey,
  api,
  onClose,
}: {
  state: EditorState;
  queryKey: string;
  api: CatalogApi;
  onClose: () => void;
}) {
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
            Esto elimina permanentemente{" "}
            <span className="font-medium text-[var(--color-foreground)]">{item?.name}</span>.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline" disabled={del.isPending}>Cancelar</Button>
          </DialogClose>
          <Button variant="destructive" onClick={() => item && del.mutate(item.id)} disabled={del.isPending || !item}>
            {del.isPending ? "Borrando…" : "Borrar"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
