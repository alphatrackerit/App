import { useState, type FormEvent, type ReactNode } from "react";
import { Link, useParams } from "react-router-dom";
import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowLeft, CheckCircle2, Coins, HeartPulse, Plus, Scale, Skull, Trash2, Truck, Wheat } from "lucide-react";
import { toast } from "sonner";
import {
  cerrarLote,
  createAlimentacion,
  createDespacho,
  createMortalidad,
  createPeso,
  createSanidad,
  deleteAlimentacion,
  deleteDespacho,
  deleteMortalidad,
  deletePeso,
  deleteSanidad,
  getIndicadoresLote,
  getLote,
  searchAlimentacion,
  searchDespachos,
  searchMortalidad,
  searchPesos,
  searchSanidad,
  getLiquidacionLote,
  TIPO_ALIMENTO,
  TIPO_SANITARIO,
  type TipoAlimento,
  type TipoRegistroSanitario,
} from "@/api/avicola";
import { DocumentosSection } from "@/components/avicola/documentos-section";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Combobox, Field, PageHero } from "@/components/list";
import { describe, formatDate } from "@/lib/list-helpers";

function fmt(n: number | null | undefined, frac = 0): string {
  if (n === null || n === undefined) return "—";
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: frac }).format(n);
}

function toNum(s: string): number | null {
  const t = s.trim();
  if (t === "") return null;
  const n = Number(t);
  return Number.isFinite(n) ? n : null;
}

function dateToIso(s: string): string {
  const t = s.trim();
  const d = t === "" ? new Date() : new Date(t);
  return Number.isNaN(d.getTime()) ? new Date().toISOString() : d.toISOString();
}

function today(): string {
  return new Date().toISOString().slice(0, 10);
}

export function LoteDetailPage() {
  const { loteId = "" } = useParams();
  const queryClient = useQueryClient();

  const loteQ = useQuery({
    queryKey: ["avicola", "lotes", "detail", loteId],
    queryFn: () => getLote(loteId),
    enabled: loteId.length > 0,
  });

  const kpiQ = useQuery({
    queryKey: ["avicola", "lotes", "indicadores", loteId],
    queryFn: () => getIndicadoresLote(loteId),
    enabled: loteId.length > 0,
  });

  const lote = loteQ.data;
  const kpi = kpiQ.data;

  const cerrar = useMutation({
    mutationFn: () => cerrarLote(loteId),
    onSuccess: () => {
      toast.success("Lote cerrado");
      queryClient.invalidateQueries({ queryKey: ["avicola", "lotes"] });
    },
    onError: (err) => toast.error("Error al cerrar", { description: describe(err) }),
  });

  return (
    <div className="space-y-6">
      <Link
        to="/avicola/lotes"
        className="inline-flex items-center gap-1.5 text-[12.5px] font-medium text-[var(--color-muted-foreground)] transition-colors hover:text-[var(--color-foreground)]"
      >
        <ArrowLeft className="size-3.5" />
        Lotes
      </Link>

      <PageHero
        eyebrow="Lote"
        title={lote?.codigo ?? (loteQ.isLoading ? "Cargando…" : "Lote")}
        subtitle={
          lote ? (
            <span className="flex flex-wrap gap-x-6 gap-y-1 tabular-nums">
              <span>Ingreso: <strong className="text-[var(--color-foreground)]">{formatDate(lote.fechaIngreso)}</strong></span>
              <span>Inicial: <strong className="text-[var(--color-foreground)]">{fmt(lote.cantidadInicial)}</strong> aves</span>
              {lote.raza && <span>Raza: <strong className="text-[var(--color-foreground)]">{lote.raza}</strong></span>}
              <span>Estado: <strong className="text-[var(--color-foreground)]">{lote.estado}</strong></span>
            </span>
          ) : (
            "—"
          )
        }
      />

      {loteQ.isError && (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(loteQ.error)}
        </div>
      )}

      {/* KPIs */}
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        <Kpi label="Edad" value={`${fmt(kpi?.edadDias)} d`} />
        <Kpi label="Aves vivas" value={fmt(kpi?.avesVivas)} hint={`de ${fmt(kpi?.cantidadInicial)}`} />
        <Kpi label="Mortalidad" value={`${fmt(kpi?.porcentajeMortalidad, 1)}%`} hint={`${fmt(kpi?.totalBajas)} bajas`} />
        <Kpi label="Viabilidad" value={`${fmt(kpi?.viabilidad, 1)}%`} />
        <Kpi label="Peso prom." value={kpi?.pesoPromedioGramos != null ? `${fmt(kpi.pesoPromedioGramos)} g` : "—"} />
        <Kpi label="Ganancia diaria" value={kpi?.gananciaDiariaGramos != null ? `${fmt(kpi.gananciaDiariaGramos, 1)} g/d` : "—"} />
        <Kpi label="Conversión (FCR)" value={kpi?.conversionAlimenticia != null ? fmt(kpi.conversionAlimenticia, 2) : "—"} />
        <Kpi label="IEP" value={kpi?.indiceEficienciaProductiva != null ? fmt(kpi.indiceEficienciaProductiva, 0) : "—"} />
        <Kpi label="Alimento" value={`${fmt(kpi?.consumoAlimentoKg, 1)} kg`} />
        <Kpi label="Despachado" value={fmt(kpi?.totalDespachado)} hint="aves" />
        <Kpi label="Costo total" value={fmt(kpi?.costoTotal, 2)} />
        <Kpi label="Ingreso despachos" value={fmt(kpi?.ingresoDespachos, 2)} />
      </div>

      {lote && lote.estado !== "Finalizado" && (
        <div>
          <Button
            variant="outline"
            onClick={() => cerrar.mutate()}
            disabled={cerrar.isPending}
            className="h-9 gap-1.5 rounded-lg px-4 text-[13px]"
          >
            <CheckCircle2 className="size-4" />
            {cerrar.isPending ? "Cerrando…" : "Cerrar lote"}
          </Button>
        </div>
      )}

      {/* Registros */}
      <MortalidadSection loteId={loteId} />
      <AlimentacionSection loteId={loteId} />
      <PesosSection loteId={loteId} />
      <SanidadSection loteId={loteId} />
      <DespachosSection loteId={loteId} />
      <DocumentosSection origen="Lote" origenId={loteId} defaultTipo="AlbaranPienso" />
      <LiquidacionSection loteId={loteId} />
    </div>
  );
}

function LiqRow({ label, value, strong, tone }: { label: string; value: string; strong?: boolean; tone?: string }) {
  return (
    <div className="flex items-center justify-between px-4 py-2">
      <span className="text-[12.5px] text-[var(--color-muted-foreground)]">{label}</span>
      <span className={`text-[13px] tabular-nums ${strong ? "font-semibold" : ""} ${tone ?? "text-[var(--color-foreground)]"}`}>{value}</span>
    </div>
  );
}

function LiquidacionSection({ loteId }: { loteId: string }) {
  const q = useQuery({
    queryKey: ["avicola", "liquidacion", loteId],
    queryFn: () => getLiquidacionLote(loteId),
    enabled: loteId.length > 0,
  });
  const l = q.data;
  return (
    <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)]">
      <header className="flex items-center gap-2 border-b border-[var(--color-border)] px-4 py-3">
        <Coins className="size-4 text-[var(--color-muted-foreground)]" />
        <h2 className="text-[14px] font-semibold text-[var(--color-foreground)]">Liquidación de la camada</h2>
      </header>
      {q.isLoading ? (
        <div className="px-4 py-6 text-center text-[12.5px] text-[var(--color-muted-foreground)]">Cargando…</div>
      ) : q.isError ? (
        <div role="alert" className="px-4 py-3 text-sm text-[var(--color-destructive)]">{describe(q.error)}</div>
      ) : l ? (
        <div className="grid grid-cols-1 divide-y divide-[var(--color-border)] sm:grid-cols-2 sm:divide-y-0">
          <div className="divide-y divide-[var(--color-border)] sm:border-r sm:border-[var(--color-border)]">
            <LiqRow label="Pollitos entrantes" value={fmt(l.pollitosEntrantes)} />
            <LiqRow label="Pollos salientes" value={fmt(l.pollosSalientes)} />
            <LiqRow label="Bajas" value={`${fmt(l.totalBajas)} · ${fmt(l.porcentajeMortalidad, 1)}%`} />
            <LiqRow label="Aves vivas" value={fmt(l.avesVivas)} />
            <LiqRow label="Pienso consumido" value={`${fmt(l.consumoAlimentoKg, 1)} kg`} />
            <LiqRow label="Peso despachado" value={`${fmt(l.pesoTotalDespachadoKg, 1)} kg`} />
            <LiqRow label="FCR" value={l.conversionAlimenticia != null ? fmt(l.conversionAlimenticia, 2) : "—"} />
            <LiqRow label="IEP" value={l.indiceEficienciaProductiva != null ? fmt(l.indiceEficienciaProductiva, 0) : "—"} />
          </div>
          <div className="divide-y divide-[var(--color-border)]">
            <LiqRow label="Costo pollitos" value={fmt(l.costoPollitos, 2)} />
            <LiqRow label="Costo pienso" value={fmt(l.costoPienso, 2)} />
            <LiqRow label="Costo sanidad" value={fmt(l.costoSanidad, 2)} />
            <LiqRow label="Costo pedidos" value={fmt(l.costoPedidos, 2)} />
            <LiqRow label="Otros costos" value={fmt(l.costoMovimientos, 2)} />
            <LiqRow label="Costo total" value={fmt(l.costoTotal, 2)} strong />
            <LiqRow label="Ingreso total" value={fmt(l.ingresoTotal, 2)} strong />
            <LiqRow label="Resultado neto" value={fmt(l.resultadoNeto, 2)} strong tone={l.resultadoNeto >= 0 ? "text-[var(--color-primary)]" : "text-[var(--color-destructive)]"} />
          </div>
        </div>
      ) : null}
    </section>
  );
}

function Kpi({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <div className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-3.5">
      <div className="text-[11.5px] font-medium uppercase tracking-wide text-[var(--color-muted-foreground)]">{label}</div>
      <div className="mt-1.5 text-[20px] font-semibold tabular-nums text-[var(--color-foreground)]">{value}</div>
      {hint && <div className="mt-0.5 text-[11px] text-[var(--color-muted-foreground)]">{hint}</div>}
    </div>
  );
}

// ───────────────────────────────────────────────────────────────────────
//  Generic registro section: header + add toggle + list with delete.
// ───────────────────────────────────────────────────────────────────────

function Section({
  title,
  icon: Icon,
  count,
  addForm,
  children,
}: {
  title: string;
  icon: React.ComponentType<{ className?: string }>;
  count: number | undefined;
  addForm: (props: { onDone: () => void }) => ReactNode;
  children: ReactNode;
}) {
  const [adding, setAdding] = useState(false);
  return (
    <section className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)]">
      <header className="flex items-center justify-between border-b border-[var(--color-border)] px-4 py-3">
        <div className="flex items-center gap-2">
          <Icon className="size-4 text-[var(--color-muted-foreground)]" />
          <h2 className="text-[14px] font-semibold text-[var(--color-foreground)]">{title}</h2>
          {count !== undefined && (
            <span className="rounded-full bg-[var(--color-muted)] px-2 py-0.5 text-[11px] tabular-nums text-[var(--color-muted-foreground)]">{count}</span>
          )}
        </div>
        <Button variant="outline" onClick={() => setAdding((a) => !a)} className="h-8 gap-1.5 rounded-lg px-3 text-[12.5px]">
          <Plus className="size-3.5" />
          Agregar
        </Button>
      </header>
      {adding && <div className="border-b border-[var(--color-border)] bg-[var(--color-muted)]/40 px-4 py-3">{addForm({ onDone: () => setAdding(false) })}</div>}
      <div className="divide-y divide-[var(--color-border)]">{children}</div>
    </section>
  );
}

function RecordRow({ left, right, onDelete }: { left: ReactNode; right: ReactNode; onDelete: () => void }) {
  return (
    <div className="group flex items-center justify-between gap-3 px-4 py-2.5">
      <div className="min-w-0 flex-1">{left}</div>
      <div className="flex items-center gap-3 text-[13px] tabular-nums text-[var(--color-foreground)]">{right}</div>
      <button
        type="button"
        aria-label="Borrar registro"
        onClick={onDelete}
        className="grid size-7 shrink-0 cursor-pointer place-items-center rounded-md text-[var(--color-muted-foreground)] opacity-0 transition-all hover:bg-[var(--color-muted)] hover:text-[var(--color-destructive)] group-hover:opacity-100"
      >
        <Trash2 className="size-3.5" />
      </button>
    </div>
  );
}

function EmptyRow({ text }: { text: string }) {
  return <div className="px-4 py-6 text-center text-[12.5px] text-[var(--color-muted-foreground)]">{text}</div>;
}

// ── Mortalidad ───────────────────────────────────────────────────────────

function MortalidadSection({ loteId }: { loteId: string }) {
  const queryClient = useQueryClient();
  const key = ["avicola", "mortalidad", loteId];
  const q = useQuery({
    queryKey: key,
    queryFn: () => searchMortalidad({ loteId, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    enabled: loteId.length > 0,
    placeholderData: keepPreviousData,
  });
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: key });
    queryClient.invalidateQueries({ queryKey: ["avicola", "lotes", "indicadores", loteId] });
  };
  const del = useMutation({
    mutationFn: (id: string) => deleteMortalidad(id),
    onSuccess: () => { toast.success("Registro borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const items = q.data?.items ?? [];

  return (
    <Section title="Mortalidad" icon={Skull} count={q.data?.totalCount} addForm={({ onDone }) => <MortalidadForm loteId={loteId} onDone={() => { onDone(); invalidate(); }} />}>
      {items.length === 0 ? <EmptyRow text="Sin registros de mortalidad." /> : items.map((m) => (
        <RecordRow
          key={m.id}
          onDelete={() => del.mutate(m.id)}
          left={<div><span className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(m.fecha)}</span>{m.causa && <span className="ml-2 text-[12.5px] text-[var(--color-muted-foreground)]">{m.causa}</span>}</div>}
          right={<><span>{fmt(m.cantidad)} bajas</span>{m.descartes ? <span className="text-[var(--color-muted-foreground)]">{fmt(m.descartes)} desc.</span> : null}</>}
        />
      ))}
    </Section>
  );
}

function MortalidadForm({ loteId, onDone }: { loteId: string; onDone: () => void }) {
  const [fecha, setFecha] = useState(today());
  const [cantidad, setCantidad] = useState("");
  const [descartes, setDescartes] = useState("");
  const [causa, setCausa] = useState("");
  const save = useMutation({
    mutationFn: () => createMortalidad({ loteId, fecha: dateToIso(fecha), cantidad: toNum(cantidad) ?? 0, descartes: toNum(descartes), causa: causa.trim() || null }),
    onSuccess: () => { toast.success("Mortalidad registrada"); onDone(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const submit = (e: FormEvent) => { e.preventDefault(); save.mutate(); };
  return (
    <form onSubmit={submit} className="flex flex-wrap items-end gap-3">
      <Field id="m-fecha" label="Fecha"><Input id="m-fecha" type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} className="w-40" /></Field>
      <Field id="m-cant" label="Bajas"><Input id="m-cant" type="number" step="1" value={cantidad} onChange={(e) => setCantidad(e.target.value)} className="w-24" placeholder="0" /></Field>
      <Field id="m-desc" label="Descartes"><Input id="m-desc" type="number" step="1" value={descartes} onChange={(e) => setDescartes(e.target.value)} className="w-24" placeholder="0" /></Field>
      <Field id="m-causa" label="Causa"><Input id="m-causa" value={causa} onChange={(e) => setCausa(e.target.value)} className="w-44" placeholder="Opcional" /></Field>
      <Button type="submit" disabled={save.isPending} className="h-9 rounded-lg px-4 text-[13px]">{save.isPending ? "…" : "Agregar"}</Button>
    </form>
  );
}

// ── Alimentación ─────────────────────────────────────────────────────────

function AlimentacionSection({ loteId }: { loteId: string }) {
  const queryClient = useQueryClient();
  const key = ["avicola", "alimentacion", loteId];
  const q = useQuery({
    queryKey: key,
    queryFn: () => searchAlimentacion({ loteId, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    enabled: loteId.length > 0,
    placeholderData: keepPreviousData,
  });
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: key });
    queryClient.invalidateQueries({ queryKey: ["avicola", "lotes", "indicadores", loteId] });
  };
  const del = useMutation({
    mutationFn: (id: string) => deleteAlimentacion(id),
    onSuccess: () => { toast.success("Registro borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const items = q.data?.items ?? [];
  return (
    <Section title="Alimentación" icon={Wheat} count={q.data?.totalCount} addForm={({ onDone }) => <AlimentacionForm loteId={loteId} onDone={() => { onDone(); invalidate(); }} />}>
      {items.length === 0 ? <EmptyRow text="Sin registros de alimentación." /> : items.map((a) => (
        <RecordRow
          key={a.id}
          onDelete={() => del.mutate(a.id)}
          left={<div><span className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(a.fecha)}</span><span className="ml-2 text-[12.5px] text-[var(--color-foreground)]">{a.tipoAlimento}</span></div>}
          right={<><span>{fmt(a.cantidadKg, 1)} kg</span>{a.costoUnitario != null ? <span className="text-[var(--color-muted-foreground)]">{fmt(a.costoUnitario, 2)}/kg</span> : null}</>}
        />
      ))}
    </Section>
  );
}

function AlimentacionForm({ loteId, onDone }: { loteId: string; onDone: () => void }) {
  const [fecha, setFecha] = useState(today());
  const [tipo, setTipo] = useState<TipoAlimento>("Iniciador");
  const [cantidadKg, setCantidadKg] = useState("");
  const [costo, setCosto] = useState("");
  const save = useMutation({
    mutationFn: () => createAlimentacion({ loteId, fecha: dateToIso(fecha), tipoAlimento: tipo, cantidadKg: toNum(cantidadKg) ?? 0, costoUnitario: toNum(costo) }),
    onSuccess: () => { toast.success("Alimentación registrada"); onDone(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const submit = (e: FormEvent) => { e.preventDefault(); save.mutate(); };
  return (
    <form onSubmit={submit} className="flex flex-wrap items-end gap-3">
      <Field id="a-fecha" label="Fecha"><Input id="a-fecha" type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} className="w-40" /></Field>
      <Field id="a-tipo" label="Tipo">
        <div className="w-40"><Combobox id="a-tipo" label="Tipo" variant="field" value={tipo} onChange={(v) => setTipo((v ?? "Iniciador") as TipoAlimento)} options={TIPO_ALIMENTO.map((t) => ({ value: t.value, label: t.label }))} /></div>
      </Field>
      <Field id="a-kg" label="Cantidad (kg)"><Input id="a-kg" type="number" step="any" value={cantidadKg} onChange={(e) => setCantidadKg(e.target.value)} className="w-28" placeholder="0" /></Field>
      <Field id="a-costo" label="Costo/kg"><Input id="a-costo" type="number" step="any" value={costo} onChange={(e) => setCosto(e.target.value)} className="w-28" placeholder="0" /></Field>
      <Button type="submit" disabled={save.isPending} className="h-9 rounded-lg px-4 text-[13px]">{save.isPending ? "…" : "Agregar"}</Button>
    </form>
  );
}

// ── Pesos ────────────────────────────────────────────────────────────────

function PesosSection({ loteId }: { loteId: string }) {
  const queryClient = useQueryClient();
  const key = ["avicola", "pesos", loteId];
  const q = useQuery({
    queryKey: key,
    queryFn: () => searchPesos({ loteId, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    enabled: loteId.length > 0,
    placeholderData: keepPreviousData,
  });
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: key });
    queryClient.invalidateQueries({ queryKey: ["avicola", "lotes", "indicadores", loteId] });
  };
  const del = useMutation({
    mutationFn: (id: string) => deletePeso(id),
    onSuccess: () => { toast.success("Registro borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const items = q.data?.items ?? [];
  return (
    <Section title="Pesos" icon={Scale} count={q.data?.totalCount} addForm={({ onDone }) => <PesoForm loteId={loteId} onDone={() => { onDone(); invalidate(); }} />}>
      {items.length === 0 ? <EmptyRow text="Sin muestreos de peso." /> : items.map((p) => (
        <RecordRow
          key={p.id}
          onDelete={() => del.mutate(p.id)}
          left={<span className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(p.fecha)}</span>}
          right={<><span>{fmt(p.pesoPromedioGramos)} g</span>{p.cantidadMuestra ? <span className="text-[var(--color-muted-foreground)]">n={fmt(p.cantidadMuestra)}</span> : null}</>}
        />
      ))}
    </Section>
  );
}

function PesoForm({ loteId, onDone }: { loteId: string; onDone: () => void }) {
  const [fecha, setFecha] = useState(today());
  const [peso, setPeso] = useState("");
  const [muestra, setMuestra] = useState("");
  const save = useMutation({
    mutationFn: () => createPeso({ loteId, fecha: dateToIso(fecha), pesoPromedioGramos: toNum(peso) ?? 0, cantidadMuestra: toNum(muestra) }),
    onSuccess: () => { toast.success("Peso registrado"); onDone(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const submit = (e: FormEvent) => { e.preventDefault(); save.mutate(); };
  return (
    <form onSubmit={submit} className="flex flex-wrap items-end gap-3">
      <Field id="p-fecha" label="Fecha"><Input id="p-fecha" type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} className="w-40" /></Field>
      <Field id="p-peso" label="Peso prom. (g)"><Input id="p-peso" type="number" step="any" value={peso} onChange={(e) => setPeso(e.target.value)} className="w-32" placeholder="0" /></Field>
      <Field id="p-muestra" label="Muestra (aves)"><Input id="p-muestra" type="number" step="1" value={muestra} onChange={(e) => setMuestra(e.target.value)} className="w-32" placeholder="0" /></Field>
      <Button type="submit" disabled={save.isPending} className="h-9 rounded-lg px-4 text-[13px]">{save.isPending ? "…" : "Agregar"}</Button>
    </form>
  );
}

// ── Sanidad ──────────────────────────────────────────────────────────────

function SanidadSection({ loteId }: { loteId: string }) {
  const queryClient = useQueryClient();
  const key = ["avicola", "sanidad", loteId];
  const q = useQuery({
    queryKey: key,
    queryFn: () => searchSanidad({ loteId, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    enabled: loteId.length > 0,
    placeholderData: keepPreviousData,
  });
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: key });
    queryClient.invalidateQueries({ queryKey: ["avicola", "lotes", "indicadores", loteId] });
  };
  const del = useMutation({
    mutationFn: (id: string) => deleteSanidad(id),
    onSuccess: () => { toast.success("Registro borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const items = q.data?.items ?? [];
  return (
    <Section title="Sanidad" icon={HeartPulse} count={q.data?.totalCount} addForm={({ onDone }) => <SanidadForm loteId={loteId} onDone={() => { onDone(); invalidate(); }} />}>
      {items.length === 0 ? <EmptyRow text="Sin registros sanitarios." /> : items.map((s) => (
        <RecordRow
          key={s.id}
          onDelete={() => del.mutate(s.id)}
          left={<div><span className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(s.fecha)}</span><span className="ml-2 text-[12.5px] text-[var(--color-foreground)]">{s.tipo}: {s.producto}</span></div>}
          right={<>{s.dosis ? <span className="text-[var(--color-muted-foreground)]">{s.dosis}</span> : null}{s.costo != null ? <span>{fmt(s.costo, 2)}</span> : null}</>}
        />
      ))}
    </Section>
  );
}

function SanidadForm({ loteId, onDone }: { loteId: string; onDone: () => void }) {
  const [fecha, setFecha] = useState(today());
  const [tipo, setTipo] = useState<TipoRegistroSanitario>("Vacunacion");
  const [producto, setProducto] = useState("");
  const [dosis, setDosis] = useState("");
  const [costo, setCosto] = useState("");
  const save = useMutation({
    mutationFn: () => createSanidad({ loteId, fecha: dateToIso(fecha), tipo, producto: producto.trim(), dosis: dosis.trim() || null, costo: toNum(costo) }),
    onSuccess: () => { toast.success("Sanidad registrada"); onDone(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const trimmed = producto.trim();
  const submit = (e: FormEvent) => { e.preventDefault(); if (!trimmed) return; save.mutate(); };
  return (
    <form onSubmit={submit} className="flex flex-wrap items-end gap-3">
      <Field id="s-fecha" label="Fecha"><Input id="s-fecha" type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} className="w-40" /></Field>
      <Field id="s-tipo" label="Tipo">
        <div className="w-40"><Combobox id="s-tipo" label="Tipo" variant="field" value={tipo} onChange={(v) => setTipo((v ?? "Vacunacion") as TipoRegistroSanitario)} options={TIPO_SANITARIO.map((t) => ({ value: t.value, label: t.label }))} /></div>
      </Field>
      <Field id="s-prod" label="Producto" required><Input id="s-prod" value={producto} onChange={(e) => setProducto(e.target.value)} className="w-44" placeholder="Gumboro" required /></Field>
      <Field id="s-dosis" label="Dosis"><Input id="s-dosis" value={dosis} onChange={(e) => setDosis(e.target.value)} className="w-28" placeholder="Opcional" /></Field>
      <Field id="s-costo" label="Costo"><Input id="s-costo" type="number" step="any" value={costo} onChange={(e) => setCosto(e.target.value)} className="w-24" placeholder="0" /></Field>
      <Button type="submit" disabled={save.isPending || !trimmed} className="h-9 rounded-lg px-4 text-[13px]">{save.isPending ? "…" : "Agregar"}</Button>
    </form>
  );
}

// ── Despachos ────────────────────────────────────────────────────────────

function DespachosSection({ loteId }: { loteId: string }) {
  const queryClient = useQueryClient();
  const key = ["avicola", "despachos", loteId];
  const q = useQuery({
    queryKey: key,
    queryFn: () => searchDespachos({ loteId, pageSize: 100, sortBy: "fecha", sortDir: "desc" }),
    enabled: loteId.length > 0,
    placeholderData: keepPreviousData,
  });
  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: key });
    queryClient.invalidateQueries({ queryKey: ["avicola", "lotes", "indicadores", loteId] });
  };
  const del = useMutation({
    mutationFn: (id: string) => deleteDespacho(id),
    onSuccess: () => { toast.success("Registro borrado"); invalidate(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const items = q.data?.items ?? [];
  return (
    <Section title="Despachos" icon={Truck} count={q.data?.totalCount} addForm={({ onDone }) => <DespachoForm loteId={loteId} onDone={() => { onDone(); invalidate(); }} />}>
      {items.length === 0 ? <EmptyRow text="Sin despachos a faena." /> : items.map((d) => (
        <RecordRow
          key={d.id}
          onDelete={() => del.mutate(d.id)}
          left={<span className="text-[13px] text-[var(--color-muted-foreground)] tabular-nums">{formatDate(d.fecha)}</span>}
          right={<><span>{fmt(d.cantidad)} aves</span><span>{fmt(d.pesoTotalKg, 1)} kg</span>{d.precioPorKg != null ? <span className="text-[var(--color-muted-foreground)]">{fmt(d.precioPorKg, 2)}/kg</span> : null}</>}
        />
      ))}
    </Section>
  );
}

function DespachoForm({ loteId, onDone }: { loteId: string; onDone: () => void }) {
  const [fecha, setFecha] = useState(today());
  const [cantidad, setCantidad] = useState("");
  const [pesoKg, setPesoKg] = useState("");
  const [precio, setPrecio] = useState("");
  const save = useMutation({
    mutationFn: () => createDespacho({ loteId, fecha: dateToIso(fecha), cantidad: toNum(cantidad) ?? 0, pesoTotalKg: toNum(pesoKg) ?? 0, precioPorKg: toNum(precio) }),
    onSuccess: () => { toast.success("Despacho registrado"); onDone(); },
    onError: (err) => toast.error("Error", { description: describe(err) }),
  });
  const submit = (e: FormEvent) => { e.preventDefault(); save.mutate(); };
  return (
    <form onSubmit={submit} className="flex flex-wrap items-end gap-3">
      <Field id="d-fecha" label="Fecha"><Input id="d-fecha" type="date" value={fecha} onChange={(e) => setFecha(e.target.value)} className="w-40" /></Field>
      <Field id="d-cant" label="Aves"><Input id="d-cant" type="number" step="1" value={cantidad} onChange={(e) => setCantidad(e.target.value)} className="w-28" placeholder="0" /></Field>
      <Field id="d-peso" label="Peso total (kg)"><Input id="d-peso" type="number" step="any" value={pesoKg} onChange={(e) => setPesoKg(e.target.value)} className="w-32" placeholder="0" /></Field>
      <Field id="d-precio" label="Precio/kg"><Input id="d-precio" type="number" step="any" value={precio} onChange={(e) => setPrecio(e.target.value)} className="w-28" placeholder="0" /></Field>
      <Button type="submit" disabled={save.isPending} className="h-9 rounded-lg px-4 text-[13px]">{save.isPending ? "…" : "Agregar"}</Button>
    </form>
  );
}
