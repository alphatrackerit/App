import { useEffect, useMemo, useState, type FormEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Check, Pencil, Plus, Sparkles, Trash2 } from "lucide-react";
import { toast } from "sonner";
import {
  completarPreparacion, createPreparacion, deletePreparacion, searchGalpones, searchLotes, searchPreparaciones, updatePreparacion,
  type EstadoPreparacion, type PreparacionDto, type PreparacionInput,
} from "@/api/avicola";
import { DocumentosSection } from "@/components/avicola/documentos-section";
import { Button } from "@/components/ui/button";
import {
  Dialog, DialogBody, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import {
  Combobox, type ComboboxOption,
  EntityEmpty, EntityListCard, EntityListHeader, EntityListLoading, EntityListRow,
  EntityPageHeader, EntityPager, Field,
} from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

const PAGE_SIZE = 20;
const COLS = "grid-cols-[1fr_150px_120px_110px_140px]";

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
function diasVacio(p: PreparacionDto): string {
  const start = p.fechaRetiro ?? p.fechaInicio;
  const end = p.fechaFin ?? new Date().toISOString();
  const ms = new Date(end).getTime() - new Date(start).getTime();
  if (!Number.isFinite(ms)) return "—";
  return `${Math.max(0, Math.floor(ms / 86400000))} d`;
}
function checklistDone(p: PreparacionDto): number {
  return [p.retiradaCama, p.lavado, p.desinfeccion, p.desinsectacion, p.camaNueva].filter(Boolean).length;
}

type EditorState =
  | { mode: "closed" }
  | { mode: "create" }
  | { mode: "edit"; prep: PreparacionDto }
  | { mode: "delete"; prep: PreparacionDto };

export function PreparacionesPage() {
  const queryClient = useQueryClient();
  const [estado, setEstado] = useState<EstadoPreparacion | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [editor, setEditor] = useState<EditorState>({ mode: "closed" });

  const galponesQ = useQuery({ queryKey: ["avicola", "galpones", "options"], queryFn: () => searchGalpones({ pageSize: 10000, sortBy: "nombre", sortDir: "asc" }) });
  const galponName = (id: string | null) => galponesQ.data?.items.find((g) => g.id === id)?.nombre ?? "—";

  const query = useQuery({
    queryKey: ["avicola", "preparaciones", "list", { estado, pageNumber }],
    queryFn: () => searchPreparaciones({ estado: estado ?? undefined, pageNumber, pageSize: PAGE_SIZE, sortBy: "fechaInicio", sortDir: "desc" }),
    placeholderData: keepPreviousData,
  });

  const completar = useMutation({
    mutationFn: (id: string) => completarPreparacion(id),
    onSuccess: () => { toast.success("Preparación completada · nave lista"); queryClient.invalidateQueries({ queryKey: ["avicola", "preparaciones"] }); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const data = query.data;
  const items = data?.items ?? [];

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Sparkles}
        title="Preparación de nave"
        total={data?.totalCount ?? null}
        unit="preparación"
        description="Vacío sanitario entre camadas: retiro, limpieza, desinfección y preparación de la nave para el próximo lote."
      >
        <Button onClick={() => setEditor({ mode: "create" })} className="h-9 flex-1 gap-1.5 rounded-lg px-4 text-[13px] font-semibold sm:flex-none">
          <Plus className="size-4" />
          Nueva preparación
        </Button>
      </EntityPageHeader>

      <div className="w-44">
        <Combobox id="prep-estado" label="Estado" variant="field" value={estado} onChange={(v) => { setEstado((v as EstadoPreparacion) ?? null); setPageNumber(1); }}
          options={[{ value: "EnProceso", label: "En proceso" }, { value: "Completada", label: "Completada" }]} clearable emptyOptionLabel="Todas" placeholder="Todas" />
      </div>

      {query.isLoading && items.length === 0 ? (
        <EntityListLoading desktopColumns={COLS} />
      ) : items.length === 0 ? (
        <EntityEmpty icon={Sparkles} title="Aún no hay preparaciones"
          body="Al retirar una camada, registra aquí la limpieza y preparación de la nave para el próximo lote."
          action={<Button onClick={() => setEditor({ mode: "create" })} className="h-9 rounded-lg px-4 text-[13px]"><Plus className="mr-1.5 size-4" />Nueva preparación</Button>} />
      ) : (
        <EntityListCard className="hidden md:block">
          <EntityListHeader className={COLS}>
            <span>Galpón</span><span>Inicio</span><span className="text-right">Días vacío</span><span>Tareas</span><span className="text-right">Acciones</span>
          </EntityListHeader>
          {items.map((p, i) => (
            <EntityListRow key={p.id} className={COLS} isLast={i === items.length - 1} onClick={() => setEditor({ mode: "edit", prep: p })}>
              <div className="min-w-0">
                <div className="truncate text-[14px] font-medium text-[var(--color-foreground)]">{galponName(p.galponId)}</div>
                <div className={`text-[11.5px] font-medium ${p.estado === "Completada" ? "text-[var(--color-primary)]" : "text-[var(--color-muted-foreground)]"}`}>{p.estado === "Completada" ? "Completada" : "En proceso"}</div>
              </div>
              <div className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(p.fechaInicio)}</div>
              <div className="text-right text-[13px] tabular-nums text-[var(--color-foreground)]">{diasVacio(p)}</div>
              <div className="text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">{checklistDone(p)}/5</div>
              <div className="flex items-center justify-end gap-1" onClick={(e) => e.stopPropagation()}>
                {p.estado === "EnProceso" && (
                  <button type="button" title="Completar" onClick={() => completar.mutate(p.id)}
                    className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-primary)]"><Check className="size-3.5" /></button>
                )}
                <button type="button" title="Editar" onClick={() => setEditor({ mode: "edit", prep: p })}
                  className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)]"><Pencil className="size-3.5" /></button>
                <button type="button" title="Borrar" onClick={() => setEditor({ mode: "delete", prep: p })}
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

      <PreparacionEditorDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
      <DeletePreparacionDialog state={editor} onClose={() => setEditor({ mode: "closed" })} />
    </div>
  );
}

function PreparacionEditorDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const prep = state.mode === "edit" ? state.prep : undefined;
  const queryClient = useQueryClient();

  const galponesQ = useQuery({ queryKey: ["avicola", "galpones", "options"], queryFn: () => searchGalpones({ pageSize: 10000, sortBy: "nombre", sortDir: "asc" }), enabled: isOpen });
  const lotesQ = useQuery({ queryKey: ["avicola", "lotes", "options"], queryFn: () => searchLotes({ pageSize: 10000, sortBy: "fechaIngreso", sortDir: "desc" }), enabled: isOpen });
  const galponOpts: ComboboxOption[] = (galponesQ.data?.items ?? []).map((g) => ({ value: g.id, label: g.nombre }));
  const loteOpts: ComboboxOption[] = (lotesQ.data?.items ?? []).map((l) => ({ value: l.id, label: l.codigo }));

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);
  const initial = useMemo(() => ({
    galponId: prep?.galponId ?? (null as string | null),
    loteAnteriorId: prep?.loteAnteriorId ?? null,
    fechaRetiro: isoToDateInput(prep?.fechaRetiro),
    fechaInicio: isoToDateInput(prep?.fechaInicio) || today,
    retiradaCama: prep?.retiradaCama ?? false,
    lavado: prep?.lavado ?? false,
    desinfeccion: prep?.desinfeccion ?? false,
    desinsectacion: prep?.desinsectacion ?? false,
    camaNueva: prep?.camaNueva ?? false,
    costo: prep?.costo?.toString() ?? "",
    estado: (prep?.estado ?? "EnProceso") as EstadoPreparacion,
    notas: prep?.notas ?? "",
  }), [prep, today]);

  const [f, setF] = useState(initial);
  useEffect(() => { if (isOpen) setF(initial); }, [isOpen, initial]);
  const set = <K extends keyof typeof initial>(k: K, v: (typeof initial)[K]) => setF((s) => ({ ...s, [k]: v }));

  const save = useMutation({
    mutationFn: (input: PreparacionInput) => (prep ? updatePreparacion(prep.id, input) : createPreparacion(input)),
    onSuccess: () => { toast.success(prep ? "Preparación actualizada" : "Preparación iniciada"); queryClient.invalidateQueries({ queryKey: ["avicola", "preparaciones"] }); onClose(); },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!f.galponId) { toast.error("Selecciona un galpón"); return; }
    const base: PreparacionInput = {
      galponId: f.galponId, loteAnteriorId: f.loteAnteriorId, fechaRetiro: dateToIso(f.fechaRetiro),
      fechaInicio: dateToIso(f.fechaInicio) ?? new Date().toISOString(),
      retiradaCama: f.retiradaCama, lavado: f.lavado, desinfeccion: f.desinfeccion, desinsectacion: f.desinsectacion, camaNueva: f.camaNueva,
      costo: toNum(f.costo), notas: f.notas.trim() || null,
    };
    save.mutate(prep ? base : { ...base, estado: f.estado });
  };

  const Tarea = ({ k, label }: { k: "retiradaCama" | "lavado" | "desinfeccion" | "desinsectacion" | "camaNueva"; label: string }) => (
    <label className="flex cursor-pointer items-center gap-2 text-[13px] text-[var(--color-foreground)]">
      <input type="checkbox" checked={f[k]} onChange={(e) => set(k, e.target.checked)} className="size-4 accent-[var(--color-primary)]" />
      {label}
    </label>
  );

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-xl">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{prep ? "Editar preparación" : "Nueva preparación de nave"}</DialogTitle>
            <DialogDescription>Vacío sanitario: limpieza y preparación de la nave entre camadas.</DialogDescription>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <Field id="pr-galpon" label="Galpón" required><Combobox id="pr-galpon" label="Galpón" variant="field" value={f.galponId} onChange={(v) => set("galponId", v)} options={galponOpts} searchable placeholder={galponesQ.isLoading ? "Cargando…" : "Selecciona"} /></Field>
              <Field id="pr-lote" label="Camada anterior"><Combobox id="pr-lote" label="Camada anterior" variant="field" value={f.loteAnteriorId} onChange={(v) => set("loteAnteriorId", v)} options={loteOpts} searchable clearable placeholder="Sin asignar" emptyOptionLabel="Sin asignar" /></Field>
              <Field id="pr-retiro" label="Fecha de retiro"><Input id="pr-retiro" type="date" value={f.fechaRetiro} onChange={(e) => set("fechaRetiro", e.target.value)} /></Field>
              <Field id="pr-inicio" label="Inicio limpieza" required><Input id="pr-inicio" type="date" value={f.fechaInicio} onChange={(e) => set("fechaInicio", e.target.value)} required /></Field>
              <Field id="pr-costo" label="Costo limpieza"><Input id="pr-costo" type="number" step="any" value={f.costo} onChange={(e) => set("costo", e.target.value)} placeholder="0" /></Field>
              {!prep && (
                <Field id="pr-estado" label="Estado"><Combobox id="pr-estado" label="Estado" variant="field" value={f.estado} onChange={(v) => set("estado", (v ?? "EnProceso") as EstadoPreparacion)} options={[{ value: "EnProceso", label: "En proceso" }, { value: "Completada", label: "Completada" }]} /></Field>
              )}
            </div>
            <div>
              <div className="mb-2 text-[12px] font-medium text-[var(--color-muted-foreground)]">Tareas de limpieza</div>
              <div className="grid grid-cols-2 gap-2.5">
                <Tarea k="retiradaCama" label="Retirada de cama" />
                <Tarea k="lavado" label="Lavado" />
                <Tarea k="desinfeccion" label="Desinfección" />
                <Tarea k="desinsectacion" label="Desinsectación" />
                <Tarea k="camaNueva" label="Cama nueva" />
              </div>
            </div>
            <Field id="pr-notas" label="Notas"><Input id="pr-notas" value={f.notas} onChange={(e) => set("notas", e.target.value)} placeholder="Opcional" /></Field>
            {prep && <DocumentosSection origen="Preparacion" origenId={prep.id} defaultTipo="CertificadoLimpieza" title="Certificado de limpieza y documentos" />}
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild><Button type="button" variant="outline" disabled={save.isPending}>Cancelar</Button></DialogClose>
            <Button type="submit" disabled={save.isPending || !f.galponId}>{save.isPending ? "Guardando…" : prep ? "Guardar" : "Crear"}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

function DeletePreparacionDialog({ state, onClose }: { state: EditorState; onClose: () => void }) {
  const isOpen = state.mode === "delete";
  const prep = state.mode === "delete" ? state.prep : undefined;
  const queryClient = useQueryClient();
  const del = useMutation({
    mutationFn: (id: string) => deletePreparacion(id),
    onSuccess: () => { toast.success("Preparación borrada"); queryClient.invalidateQueries({ queryKey: ["avicola", "preparaciones"] }); onClose(); },
    onError: (err) => toast.error("Error al borrar", { description: describe(err) }),
  });
  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="text-[var(--color-destructive)]">Borrar preparación</DialogTitle>
          <DialogDescription>Esto elimina permanentemente este registro de preparación de nave.</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <DialogClose asChild><Button type="button" variant="outline" disabled={del.isPending}>Cancelar</Button></DialogClose>
          <Button variant="destructive" onClick={() => prep && del.mutate(prep.id)} disabled={del.isPending || !prep}>{del.isPending ? "Borrando…" : "Borrar"}</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
