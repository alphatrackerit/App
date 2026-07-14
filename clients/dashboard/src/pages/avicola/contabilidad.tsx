import { useEffect, useMemo, useState, type FormEvent } from "react";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Calculator, Pencil, Plus, Trash2, TrendingDown, TrendingUp, Wallet } from "lucide-react";
import { toast } from "sonner";
import {
  createMovimiento, deleteMovimiento, getContabilidad, searchLotes, searchMovimientos, updateMovimiento,
  CATEGORIA_MOVIMIENTO, TIPO_MOVIMIENTO,
  type CategoriaMovimiento, type MovimientoDto, type MovimientoInput, type TipoMovimiento,
} from "@/api/avicola";
import { Button } from "@/components/ui/button";
import {
  Dialog, DialogBody, DialogClose, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Combobox, type ComboboxOption, EntityListCard, EntityListHeader, EntityListRow, Field } from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

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
const catLabel = (c: CategoriaMovimiento) => CATEGORIA_MOVIMIENTO.find((x) => x.value === c)?.label ?? c;

type MovEditor = { mode: "closed" } | { mode: "create" } | { mode: "edit"; mov: MovimientoDto };

export function ContabilidadPage() {
  const queryClient = useQueryClient();
  const [loteId, setLoteId] = useState<string | null>(null);
  const [movEditor, setMovEditor] = useState<MovEditor>({ mode: "closed" });

  const lotesQ = useQuery({ queryKey: ["avicola", "lotes", "options"], queryFn: () => searchLotes({ pageSize: 200, sortBy: "fechaIngreso", sortDir: "desc" }) });
  const loteOpts: ComboboxOption[] = (lotesQ.data?.items ?? []).map((l) => ({ value: l.id, label: l.codigo }));

  const cont = useQuery({
    queryKey: ["avicola", "contabilidad", { loteId }],
    queryFn: () => getContabilidad({ loteId: loteId ?? undefined }),
  });

  const movKey = ["avicola", "movimientos", { loteId }];
  const movs = useQuery({
    queryKey: movKey,
    queryFn: () => searchMovimientos({ loteId: loteId ?? undefined, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    placeholderData: keepPreviousData,
  });
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["avicola", "contabilidad"] });
    queryClient.invalidateQueries({ queryKey: ["avicola", "movimientos"] });
  };
  const delMov = useMutation({
    mutationFn: (id: string) => deleteMovimiento(id),
    onSuccess: () => { toast.success("Movimiento borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });

  const c = cont.data;
  const movItems = movs.data?.items ?? [];

  return (
    <div className="space-y-4 sm:space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div className="flex items-center gap-2.5">
          <Calculator className="size-5 text-[var(--color-muted-foreground)]" />
          <div>
            <h1 className="text-[20px] font-semibold text-[var(--color-foreground)]">Contabilidad</h1>
            <p className="text-[12.5px] text-[var(--color-muted-foreground)]">Costos e ingresos por categoría y por lote, con el libro de movimientos manuales.</p>
          </div>
        </div>
        <div className="w-52">
          <Combobox id="cont-lote" label="Lote" variant="field" value={loteId} onChange={setLoteId} options={loteOpts} searchable clearable placeholder="Todos los lotes" emptyOptionLabel="Todos los lotes" />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
        <StatCard icon={TrendingUp} label="Ingresos" value={money(c?.totalIngresos)} tone="text-[var(--color-primary)]" />
        <StatCard icon={TrendingDown} label="Egresos / costos" value={money(c?.totalEgresos)} tone="text-[var(--color-foreground)]" />
        <StatCard icon={Wallet} label="Resultado" value={money(c?.resultado)} tone={(c?.resultado ?? 0) >= 0 ? "text-[var(--color-primary)]" : "text-[var(--color-destructive)]"} />
      </div>

      {cont.isError && (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">{describe(cont.error)}</div>
      )}

      {/* Por categoría */}
      <EntityListCard className="hidden md:block">
        <EntityListHeader className="grid-cols-[1fr_140px_140px]"><span>Categoría</span><span className="text-right">Ingresos</span><span className="text-right">Egresos</span></EntityListHeader>
        {(c?.porCategoria ?? []).length === 0 ? (
          <div className="px-4 py-6 text-center text-[12.5px] text-[var(--color-muted-foreground)]">Sin datos contables todavía.</div>
        ) : (c?.porCategoria ?? []).map((row, i, arr) => (
          <EntityListRow key={row.categoria} className="grid-cols-[1fr_140px_140px]" isLast={i === arr.length - 1}>
            <div className="text-[13px] text-[var(--color-foreground)]">{catLabel(row.categoria)}</div>
            <div className="text-right text-[13px] tabular-nums text-[var(--color-primary)]">{row.ingresos ? money(row.ingresos) : "—"}</div>
            <div className="text-right text-[13px] tabular-nums">{row.egresos ? money(row.egresos) : "—"}</div>
          </EntityListRow>
        ))}
      </EntityListCard>

      {/* Por lote */}
      {!loteId && (
        <EntityListCard className="hidden md:block">
          <EntityListHeader className="grid-cols-[1fr_130px_130px_130px]"><span>Lote</span><span className="text-right">Costos</span><span className="text-right">Ingresos</span><span className="text-right">Resultado</span></EntityListHeader>
          {(c?.porLote ?? []).length === 0 ? (
            <div className="px-4 py-6 text-center text-[12.5px] text-[var(--color-muted-foreground)]">Sin lotes.</div>
          ) : (c?.porLote ?? []).map((row, i, arr) => (
            <EntityListRow key={row.loteId} className="grid-cols-[1fr_130px_130px_130px]" isLast={i === arr.length - 1}>
              <div className="text-[13px] font-medium text-[var(--color-foreground)]">{row.codigo}</div>
              <div className="text-right text-[13px] tabular-nums">{money(row.costos)}</div>
              <div className="text-right text-[13px] tabular-nums text-[var(--color-primary)]">{money(row.ingresos)}</div>
              <div className={`text-right text-[13px] font-medium tabular-nums ${row.resultado >= 0 ? "text-[var(--color-primary)]" : "text-[var(--color-destructive)]"}`}>{money(row.resultado)}</div>
            </EntityListRow>
          ))}
        </EntityListCard>
      )}

      {/* Libro de movimientos */}
      <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)]">
        <header className="flex items-center justify-between border-b border-[var(--color-border)] px-4 py-3">
          <h2 className="text-[14px] font-semibold text-[var(--color-foreground)]">Movimientos manuales</h2>
          <Button variant="outline" onClick={() => setMovEditor({ mode: "create" })} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]"><Plus className="size-3.5" />Nuevo movimiento</Button>
        </header>
        <div className="divide-y divide-[var(--color-border)]">
          {movItems.length === 0 ? (
            <div className="px-4 py-6 text-center text-[12.5px] text-[var(--color-muted-foreground)]">Sin movimientos manuales.</div>
          ) : movItems.map((m) => (
            <div key={m.id} className="group flex items-center justify-between gap-3 px-4 py-2.5">
              <div className="min-w-0">
                <div className="truncate text-[13px] text-[var(--color-foreground)]">{m.concepto}</div>
                <div className="text-[11.5px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(m.fecha)} · {catLabel(m.categoria)}</div>
              </div>
              <div className={`text-[13px] font-medium tabular-nums ${m.tipo === "Ingreso" ? "text-[var(--color-primary)]" : "text-[var(--color-foreground)]"}`}>{m.tipo === "Ingreso" ? "+" : "−"}{money(m.importe)}</div>
              <div className="flex items-center gap-1">
                <button type="button" title="Editar" onClick={() => setMovEditor({ mode: "edit", mov: m })} className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-foreground)] group-hover:opacity-100"><Pencil className="size-3.5" /></button>
                <button type="button" title="Borrar" onClick={() => delMov.mutate(m.id)} className="grid size-7 place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)] group-hover:opacity-100"><Trash2 className="size-3.5" /></button>
              </div>
            </div>
          ))}
        </div>
      </section>

      <MovimientoDialog state={movEditor} loteId={loteId} onClose={() => setMovEditor({ mode: "closed" })} onSaved={invalidate} />
    </div>
  );
}

function StatCard({ icon: Icon, label, value, tone }: { icon: React.ComponentType<{ className?: string }>; label: string; value: string; tone: string }) {
  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4">
      <div className="flex items-center gap-2 text-[12px] font-medium text-[var(--color-muted-foreground)]"><Icon className="size-4" />{label}</div>
      <div className={`mt-2 text-[24px] font-semibold tabular-nums ${tone}`}>{value}</div>
    </div>
  );
}

function MovimientoDialog({ state, loteId, onClose, onSaved }: { state: MovEditor; loteId: string | null; onClose: () => void; onSaved: () => void }) {
  const isOpen = state.mode === "create" || state.mode === "edit";
  const mov = state.mode === "edit" ? state.mov : undefined;

  const today = useMemo(() => new Date().toISOString().slice(0, 10), []);
  const initial = useMemo(() => ({
    fecha: mov ? new Date(mov.fecha).toISOString().slice(0, 10) : today,
    tipo: (mov?.tipo ?? "Egreso") as TipoMovimiento,
    categoria: (mov?.categoria ?? "Otro") as CategoriaMovimiento,
    concepto: mov?.concepto ?? "",
    importe: mov?.importe?.toString() ?? "",
    notas: mov?.notas ?? "",
  }), [mov, today]);

  const [f, setF] = useState(initial);
  useEffect(() => { if (isOpen) setF(initial); }, [isOpen, initial]);
  const set = <K extends keyof typeof initial>(k: K, v: (typeof initial)[K]) => setF((s) => ({ ...s, [k]: v }));

  const save = useMutation({
    mutationFn: (input: MovimientoInput) => (mov ? updateMovimiento(mov.id, input) : createMovimiento(input)),
    onSuccess: () => { toast.success(mov ? "Movimiento actualizado" : "Movimiento creado"); onSaved(); onClose(); },
    onError: (err) => toast.error("Error al guardar", { description: describe(err) }),
  });

  const trimmed = f.concepto.trim();
  const onSubmit = (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trimmed) return;
    save.mutate({
      fecha: dateToIso(f.fecha) ?? new Date().toISOString(), tipo: f.tipo, categoria: f.categoria,
      concepto: trimmed, importe: toNum(f.importe) ?? 0, loteId: loteId ?? mov?.loteId ?? null, notas: f.notas.trim() || null,
    });
  };

  return (
    <Dialog open={isOpen} onOpenChange={(o) => (!o ? onClose() : undefined)}>
      <DialogContent className="!max-w-lg">
        <form onSubmit={onSubmit}>
          <DialogHeader>
            <DialogTitle>{mov ? "Editar movimiento" : "Nuevo movimiento"}</DialogTitle>
            <DialogDescription>Registra un ingreso o gasto manual{loteId ? " para el lote seleccionado" : ""}.</DialogDescription>
          </DialogHeader>
          <DialogBody className="space-y-5">
            <div className="grid grid-cols-2 gap-4">
              <Field id="mv-fecha" label="Fecha"><Input id="mv-fecha" type="date" value={f.fecha} onChange={(e) => set("fecha", e.target.value)} /></Field>
              <Field id="mv-tipo" label="Tipo"><Combobox id="mv-tipo" label="Tipo" variant="field" value={f.tipo} onChange={(v) => set("tipo", (v ?? "Egreso") as TipoMovimiento)} options={TIPO_MOVIMIENTO.map((t) => ({ value: t.value, label: t.label }))} /></Field>
              <Field id="mv-cat" label="Categoría"><Combobox id="mv-cat" label="Categoría" variant="field" value={f.categoria} onChange={(v) => set("categoria", (v ?? "Otro") as CategoriaMovimiento)} options={CATEGORIA_MOVIMIENTO.map((cm) => ({ value: cm.value, label: cm.label }))} /></Field>
              <Field id="mv-importe" label="Importe"><Input id="mv-importe" type="number" step="any" value={f.importe} onChange={(e) => set("importe", e.target.value)} placeholder="0" /></Field>
            </div>
            <Field id="mv-concepto" label="Concepto" required><Input id="mv-concepto" value={f.concepto} onChange={(e) => set("concepto", e.target.value)} placeholder="Compra de pienso / venta…" required /></Field>
            <Field id="mv-notas" label="Notas"><Input id="mv-notas" value={f.notas} onChange={(e) => set("notas", e.target.value)} placeholder="Opcional" /></Field>
          </DialogBody>
          <DialogFooter>
            <DialogClose asChild><Button type="button" variant="outline" disabled={save.isPending}>Cancelar</Button></DialogClose>
            <Button type="submit" disabled={save.isPending || !trimmed}>{save.isPending ? "Guardando…" : mov ? "Guardar" : "Crear"}</Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
