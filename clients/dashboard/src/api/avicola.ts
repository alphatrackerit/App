import { apiFetch } from "@/lib/api-client";

// ───────────────────────────────────────────────────────────────────────
//  Avícola (broiler poultry farm) — pollos de engorde
//  Mirrors the FSH.Modules.Avicola backend contracts. JSON is camelCase;
//  enums are serialized as their string names.
// ───────────────────────────────────────────────────────────────────────

export type PagedResponse<T> = {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
};

export type SortDir = "asc" | "desc";

type PagedParams = {
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: SortDir;
};

function query(params: Record<string, unknown>): string {
  const q = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null || value === "") continue;
    q.set(key, String(value));
  }
  const s = q.toString();
  return s ? `?${s}` : "";
}

const BASE = "/api/v1/avicola";

export type EstadoLote = "Planificado" | "EnCrianza" | "Finalizado" | "Cancelado";
export type TipoAlimento = "Iniciador" | "Crecimiento" | "Engorde" | "Final";
export type TipoRegistroSanitario = "Vacunacion" | "Medicacion" | "Tratamiento" | "Vitaminas";

export const ESTADO_LOTE: { value: EstadoLote; label: string }[] = [
  { value: "Planificado", label: "Planificado" },
  { value: "EnCrianza", label: "En crianza" },
  { value: "Finalizado", label: "Finalizado" },
  { value: "Cancelado", label: "Cancelado" },
];

export const TIPO_ALIMENTO: { value: TipoAlimento; label: string }[] = [
  { value: "Iniciador", label: "Iniciador" },
  { value: "Crecimiento", label: "Crecimiento" },
  { value: "Engorde", label: "Engorde" },
  { value: "Final", label: "Final" },
];

export const TIPO_SANITARIO: { value: TipoRegistroSanitario; label: string }[] = [
  { value: "Vacunacion", label: "Vacunación" },
  { value: "Medicacion", label: "Medicación" },
  { value: "Tratamiento", label: "Tratamiento" },
  { value: "Vitaminas", label: "Vitaminas" },
];

// ── Galpones (sheds) ─────────────────────────────────────────────────────

export type GalponDto = {
  id: string;
  nombre: string;
  codigo: string | null;
  capacidad: number;
  superficieM2: number | null;
  ubicacion: string | null;
  activo: boolean;
  notas: string | null;
};

export type GalponInput = {
  nombre: string;
  codigo?: string | null;
  capacidad?: number;
  superficieM2?: number | null;
  ubicacion?: string | null;
  activo?: boolean;
  notas?: string | null;
};

export function searchGalpones(
  params: PagedParams & { search?: string; activo?: boolean } = {},
): Promise<PagedResponse<GalponDto>> {
  return apiFetch<PagedResponse<GalponDto>>(`${BASE}/galpones${query(params)}`);
}

export function getGalpon(id: string): Promise<GalponDto> {
  return apiFetch<GalponDto>(`${BASE}/galpones/${encodeURIComponent(id)}`);
}

export function createGalpon(input: GalponInput): Promise<string> {
  return apiFetch<string>(`${BASE}/galpones`, { method: "POST", body: JSON.stringify(input) });
}

export function updateGalpon(id: string, input: GalponInput): Promise<string> {
  return apiFetch<string>(`${BASE}/galpones/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteGalpon(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/galpones/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Lotes (flocks) ───────────────────────────────────────────────────────

export type LoteDto = {
  id: string;
  codigo: string;
  galponId: string | null;
  raza: string | null;
  fechaIngreso: string;
  cantidadInicial: number;
  pesoInicialGramos: number | null;
  fechaSalidaPrevista: string | null;
  fechaSalidaReal: string | null;
  estado: EstadoLote;
  proveedorId: string | null;
  costoPolluelo: number | null;
  notas: string | null;
};

export type LoteInput = {
  codigo: string;
  galponId?: string | null;
  raza?: string | null;
  fechaIngreso?: string;
  cantidadInicial?: number;
  pesoInicialGramos?: number | null;
  fechaSalidaPrevista?: string | null;
  estado?: EstadoLote;
  proveedorId?: string | null;
  costoPolluelo?: number | null;
  notas?: string | null;
};

export function searchLotes(
  params: PagedParams & { search?: string; galponId?: string; estado?: EstadoLote } = {},
): Promise<PagedResponse<LoteDto>> {
  return apiFetch<PagedResponse<LoteDto>>(`${BASE}/lotes${query(params)}`);
}

export function getLote(id: string): Promise<LoteDto> {
  return apiFetch<LoteDto>(`${BASE}/lotes/${encodeURIComponent(id)}`);
}

export function createLote(input: LoteInput): Promise<string> {
  return apiFetch<string>(`${BASE}/lotes`, { method: "POST", body: JSON.stringify(input) });
}

export function updateLote(id: string, input: LoteInput): Promise<string> {
  return apiFetch<string>(`${BASE}/lotes/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteLote(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/lotes/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export function cerrarLote(id: string, fechaSalidaReal?: string | null): Promise<string> {
  return apiFetch<string>(`${BASE}/lotes/${encodeURIComponent(id)}/cerrar`, {
    method: "POST",
    body: JSON.stringify({ fechaSalidaReal: fechaSalidaReal ?? null }),
  });
}

// ── Mortalidad ───────────────────────────────────────────────────────────

export type MortalidadDto = {
  id: string;
  loteId: string;
  fecha: string;
  cantidad: number;
  descartes: number | null;
  causa: string | null;
  notas: string | null;
};

export type MortalidadInput = {
  loteId: string;
  fecha: string;
  cantidad: number;
  descartes?: number | null;
  causa?: string | null;
  notas?: string | null;
};

export function searchMortalidad(
  params: PagedParams & { loteId?: string } = {},
): Promise<PagedResponse<MortalidadDto>> {
  return apiFetch<PagedResponse<MortalidadDto>>(`${BASE}/mortalidad${query(params)}`);
}

export function createMortalidad(input: MortalidadInput): Promise<string> {
  return apiFetch<string>(`${BASE}/mortalidad`, { method: "POST", body: JSON.stringify(input) });
}

export function updateMortalidad(id: string, input: MortalidadInput): Promise<string> {
  return apiFetch<string>(`${BASE}/mortalidad/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteMortalidad(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/mortalidad/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Alimentación (feed) ──────────────────────────────────────────────────

export type AlimentacionDto = {
  id: string;
  loteId: string;
  fecha: string;
  tipoAlimento: TipoAlimento;
  cantidadKg: number;
  costoUnitario: number | null;
  notas: string | null;
};

export type AlimentacionInput = {
  loteId: string;
  fecha: string;
  tipoAlimento: TipoAlimento;
  cantidadKg: number;
  costoUnitario?: number | null;
  notas?: string | null;
};

export function searchAlimentacion(
  params: PagedParams & { loteId?: string; tipoAlimento?: TipoAlimento } = {},
): Promise<PagedResponse<AlimentacionDto>> {
  return apiFetch<PagedResponse<AlimentacionDto>>(`${BASE}/alimentacion${query(params)}`);
}

export function createAlimentacion(input: AlimentacionInput): Promise<string> {
  return apiFetch<string>(`${BASE}/alimentacion`, { method: "POST", body: JSON.stringify(input) });
}

export function updateAlimentacion(id: string, input: AlimentacionInput): Promise<string> {
  return apiFetch<string>(`${BASE}/alimentacion/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteAlimentacion(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/alimentacion/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Pesos (weights) ──────────────────────────────────────────────────────

export type PesoDto = {
  id: string;
  loteId: string;
  fecha: string;
  pesoPromedioGramos: number;
  cantidadMuestra: number | null;
  notas: string | null;
};

export type PesoInput = {
  loteId: string;
  fecha: string;
  pesoPromedioGramos: number;
  cantidadMuestra?: number | null;
  notas?: string | null;
};

export function searchPesos(
  params: PagedParams & { loteId?: string } = {},
): Promise<PagedResponse<PesoDto>> {
  return apiFetch<PagedResponse<PesoDto>>(`${BASE}/pesos${query(params)}`);
}

export function createPeso(input: PesoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/pesos`, { method: "POST", body: JSON.stringify(input) });
}

export function updatePeso(id: string, input: PesoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/pesos/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deletePeso(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/pesos/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Sanidad (health) ─────────────────────────────────────────────────────

export type SanidadDto = {
  id: string;
  loteId: string;
  fecha: string;
  tipo: TipoRegistroSanitario;
  producto: string;
  dosis: string | null;
  viaAplicacion: string | null;
  costo: number | null;
  notas: string | null;
};

export type SanidadInput = {
  loteId: string;
  fecha: string;
  tipo: TipoRegistroSanitario;
  producto: string;
  dosis?: string | null;
  viaAplicacion?: string | null;
  costo?: number | null;
  notas?: string | null;
};

export function searchSanidad(
  params: PagedParams & { loteId?: string; tipo?: TipoRegistroSanitario } = {},
): Promise<PagedResponse<SanidadDto>> {
  return apiFetch<PagedResponse<SanidadDto>>(`${BASE}/sanidad${query(params)}`);
}

export function createSanidad(input: SanidadInput): Promise<string> {
  return apiFetch<string>(`${BASE}/sanidad`, { method: "POST", body: JSON.stringify(input) });
}

export function updateSanidad(id: string, input: SanidadInput): Promise<string> {
  return apiFetch<string>(`${BASE}/sanidad/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteSanidad(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/sanidad/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Despachos (harvest / dispatch) ───────────────────────────────────────

export type DespachoDto = {
  id: string;
  loteId: string;
  fecha: string;
  cantidad: number;
  pesoTotalKg: number;
  precioPorKg: number | null;
  clienteId: string | null;
  notas: string | null;
};

export type DespachoInput = {
  loteId: string;
  fecha: string;
  cantidad: number;
  pesoTotalKg: number;
  precioPorKg?: number | null;
  clienteId?: string | null;
  notas?: string | null;
};

export function searchDespachos(
  params: PagedParams & { loteId?: string } = {},
): Promise<PagedResponse<DespachoDto>> {
  return apiFetch<PagedResponse<DespachoDto>>(`${BASE}/despachos${query(params)}`);
}

export function createDespacho(input: DespachoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/despachos`, { method: "POST", body: JSON.stringify(input) });
}

export function updateDespacho(id: string, input: DespachoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/despachos/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteDespacho(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/despachos/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Indicadores (KPIs) ───────────────────────────────────────────────────

export type IndicadoresLote = {
  loteId: string;
  codigo: string;
  estado: EstadoLote;
  fechaIngreso: string;
  edadDias: number;
  cantidadInicial: number;
  totalBajas: number;
  totalDespachado: number;
  avesVivas: number;
  porcentajeMortalidad: number;
  viabilidad: number;
  consumoAlimentoKg: number;
  pesoPromedioGramos: number | null;
  gananciaDiariaGramos: number | null;
  conversionAlimenticia: number | null;
  indiceEficienciaProductiva: number | null;
  costoPolluelos: number;
  costoAlimento: number;
  costoSanidad: number;
  costoTotal: number;
  ingresoDespachos: number;
};

export type LoteResumen = {
  id: string;
  codigo: string;
  estado: EstadoLote;
  galponId: string | null;
  fechaIngreso: string;
  edadDias: number;
  cantidadInicial: number;
  avesVivas: number;
  porcentajeMortalidad: number;
  consumoAlimentoKg: number;
  pesoPromedioGramos: number | null;
  conversionAlimenticia: number | null;
};

export type ResumenAvicola = {
  lotes: LoteResumen[];
  totalLotes: number;
  lotesActivos: number;
  totalAvesVivas: number;
  consumoAlimentoTotalKg: number;
};

export function getIndicadoresLote(id: string): Promise<IndicadoresLote> {
  return apiFetch<IndicadoresLote>(`${BASE}/lotes/${encodeURIComponent(id)}/indicadores`);
}

export function getResumenAvicola(): Promise<ResumenAvicola> {
  return apiFetch<ResumenAvicola>(`${BASE}/resumen`);
}

// ───────────────────────────────────────────────────────────────────────
//  Gestión: documentos, pedidos, contabilidad
// ───────────────────────────────────────────────────────────────────────

export type TipoDocumento =
  | "AlbaranPienso" | "CertificadoLimpieza" | "Mortalidad" | "Sanidad" | "Pedido" | "Factura" | "Otro";
export type DocumentoOrigen = "Lote" | "Galpon" | "Pedido" | "Movimiento" | "Preparacion" | "General";
export type EstadoPreparacion = "EnProceso" | "Completada";
export type TipoPedido = "Pienso" | "Pollitos" | "Medicamento" | "Insumo" | "Otro";
export type EstadoPedido = "Borrador" | "Enviado" | "Recibido" | "Cancelado";
export type AccionPedido = "Enviar" | "Recibir" | "Cancelar";
export type TipoMovimiento = "Ingreso" | "Egreso";
export type CategoriaMovimiento =
  | "Pollitos" | "Pienso" | "Sanidad" | "ManoDeObra" | "Servicios" | "Transporte" | "VentaPollos" | "Otro";

export const TIPO_DOCUMENTO: { value: TipoDocumento; label: string }[] = [
  { value: "AlbaranPienso", label: "Albarán de pienso" },
  { value: "CertificadoLimpieza", label: "Certificado de limpieza" },
  { value: "Mortalidad", label: "Mortalidad" },
  { value: "Sanidad", label: "Sanidad" },
  { value: "Pedido", label: "Pedido" },
  { value: "Factura", label: "Factura" },
  { value: "Otro", label: "Otro" },
];

export const TIPO_PEDIDO: { value: TipoPedido; label: string }[] = [
  { value: "Pienso", label: "Pienso" },
  { value: "Pollitos", label: "Pollitos" },
  { value: "Medicamento", label: "Medicamento" },
  { value: "Insumo", label: "Insumo" },
  { value: "Otro", label: "Otro" },
];

export const ESTADO_PEDIDO: { value: EstadoPedido; label: string }[] = [
  { value: "Borrador", label: "Borrador" },
  { value: "Enviado", label: "Enviado" },
  { value: "Recibido", label: "Recibido" },
  { value: "Cancelado", label: "Cancelado" },
];

export const TIPO_MOVIMIENTO: { value: TipoMovimiento; label: string }[] = [
  { value: "Ingreso", label: "Ingreso" },
  { value: "Egreso", label: "Egreso" },
];

export const CATEGORIA_MOVIMIENTO: { value: CategoriaMovimiento; label: string }[] = [
  { value: "Pollitos", label: "Pollitos" },
  { value: "Pienso", label: "Pienso" },
  { value: "Sanidad", label: "Sanidad" },
  { value: "ManoDeObra", label: "Mano de obra" },
  { value: "Servicios", label: "Servicios" },
  { value: "Transporte", label: "Transporte" },
  { value: "VentaPollos", label: "Venta de pollos" },
  { value: "Otro", label: "Otro" },
];

// ── Documentos ───────────────────────────────────────────────────────────

export type DocumentoDto = {
  id: string;
  tipo: TipoDocumento;
  origen: DocumentoOrigen;
  origenId: string | null;
  fileAssetId: string | null;
  url: string;
  nombreArchivo: string;
  contentType: string | null;
  fecha: string;
  notas: string | null;
};

export type DocumentoInput = {
  tipo: TipoDocumento;
  origen: DocumentoOrigen;
  origenId?: string | null;
  fileAssetId?: string | null;
  url: string;
  nombreArchivo: string;
  contentType?: string | null;
  fecha?: string | null;
  notas?: string | null;
};

export function searchDocumentos(
  params: PagedParams & { origen?: DocumentoOrigen; origenId?: string; tipo?: TipoDocumento } = {},
): Promise<PagedResponse<DocumentoDto>> {
  return apiFetch<PagedResponse<DocumentoDto>>(`${BASE}/documentos${query(params)}`);
}

export function createDocumento(input: DocumentoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/documentos`, { method: "POST", body: JSON.stringify(input) });
}

export function updateDocumento(id: string, input: { tipo: TipoDocumento; notas?: string | null }): Promise<string> {
  return apiFetch<string>(`${BASE}/documentos/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteDocumento(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/documentos/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Pedidos ──────────────────────────────────────────────────────────────

export type PedidoDto = {
  id: string;
  codigo: string;
  tipo: TipoPedido;
  proveedorId: string | null;
  descripcion: string | null;
  cantidad: number;
  unidad: string | null;
  costoEstimado: number | null;
  costoReal: number | null;
  estado: EstadoPedido;
  fechaPedido: string;
  fechaRecepcion: string | null;
  loteId: string | null;
  galponId: string | null;
  notas: string | null;
};

export type PedidoInput = {
  codigo: string;
  tipo: TipoPedido;
  proveedorId?: string | null;
  descripcion?: string | null;
  cantidad?: number;
  unidad?: string | null;
  costoEstimado?: number | null;
  estado?: EstadoPedido;
  fechaPedido?: string;
  loteId?: string | null;
  galponId?: string | null;
  notas?: string | null;
};

export function searchPedidos(
  params: PagedParams & { search?: string; tipo?: TipoPedido; estado?: EstadoPedido; loteId?: string } = {},
): Promise<PagedResponse<PedidoDto>> {
  return apiFetch<PagedResponse<PedidoDto>>(`${BASE}/pedidos${query(params)}`);
}

export function getPedido(id: string): Promise<PedidoDto> {
  return apiFetch<PedidoDto>(`${BASE}/pedidos/${encodeURIComponent(id)}`);
}

export function createPedido(input: PedidoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/pedidos`, { method: "POST", body: JSON.stringify(input) });
}

export function updatePedido(id: string, input: PedidoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/pedidos/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deletePedido(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/pedidos/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export function cambiarEstadoPedido(
  id: string,
  accion: AccionPedido,
  extra: { fechaRecepcion?: string | null; costoReal?: number | null } = {},
): Promise<string> {
  return apiFetch<string>(`${BASE}/pedidos/${encodeURIComponent(id)}/estado`, {
    method: "POST",
    body: JSON.stringify({ accion, fechaRecepcion: extra.fechaRecepcion ?? null, costoReal: extra.costoReal ?? null }),
  });
}

// ── Movimientos (manual ledger) ──────────────────────────────────────────

export type MovimientoDto = {
  id: string;
  fecha: string;
  tipo: TipoMovimiento;
  categoria: CategoriaMovimiento;
  concepto: string;
  importe: number;
  loteId: string | null;
  notas: string | null;
};

export type MovimientoInput = {
  fecha: string;
  tipo: TipoMovimiento;
  categoria: CategoriaMovimiento;
  concepto: string;
  importe: number;
  loteId?: string | null;
  notas?: string | null;
};

export function searchMovimientos(
  params: PagedParams & { tipo?: TipoMovimiento; categoria?: CategoriaMovimiento; loteId?: string } = {},
): Promise<PagedResponse<MovimientoDto>> {
  return apiFetch<PagedResponse<MovimientoDto>>(`${BASE}/movimientos${query(params)}`);
}

export function createMovimiento(input: MovimientoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/movimientos`, { method: "POST", body: JSON.stringify(input) });
}

export function updateMovimiento(id: string, input: MovimientoInput): Promise<string> {
  return apiFetch<string>(`${BASE}/movimientos/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deleteMovimiento(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/movimientos/${encodeURIComponent(id)}`, { method: "DELETE" });
}

// ── Contabilidad + liquidación ───────────────────────────────────────────

export type ContabilidadCategoria = { categoria: CategoriaMovimiento; ingresos: number; egresos: number };
export type ContabilidadLote = {
  loteId: string;
  codigo: string;
  estado: EstadoLote;
  costos: number;
  ingresos: number;
  resultado: number;
};
export type Contabilidad = {
  porCategoria: ContabilidadCategoria[];
  porLote: ContabilidadLote[];
  totalIngresos: number;
  totalEgresos: number;
  resultado: number;
};

export type LiquidacionLote = {
  loteId: string;
  codigo: string;
  estado: EstadoLote;
  fechaIngreso: string;
  fechaSalidaReal: string | null;
  edadDias: number;
  pollitosEntrantes: number;
  pollosSalientes: number;
  avesVivas: number;
  totalBajas: number;
  porcentajeMortalidad: number;
  viabilidad: number;
  consumoAlimentoKg: number;
  pesoTotalDespachadoKg: number;
  pesoPromedioGramos: number | null;
  conversionAlimenticia: number | null;
  indiceEficienciaProductiva: number | null;
  costoPollitos: number;
  costoPienso: number;
  costoSanidad: number;
  costoPedidos: number;
  costoMovimientos: number;
  costoTotal: number;
  ingresoDespachos: number;
  ingresoMovimientos: number;
  ingresoTotal: number;
  resultadoNeto: number;
  margenPorAve: number | null;
  margenPorKg: number | null;
};

export function getContabilidad(
  params: { loteId?: string; desde?: string; hasta?: string } = {},
): Promise<Contabilidad> {
  return apiFetch<Contabilidad>(`${BASE}/contabilidad${query(params)}`);
}

export function getLiquidacionLote(loteId: string): Promise<LiquidacionLote> {
  return apiFetch<LiquidacionLote>(`${BASE}/lotes/${encodeURIComponent(loteId)}/liquidacion`);
}

// ── Preparación de nave (vacío sanitario entre camadas) ──────────────────

export type PreparacionDto = {
  id: string;
  galponId: string;
  loteAnteriorId: string | null;
  fechaRetiro: string | null;
  fechaInicio: string;
  fechaFin: string | null;
  retiradaCama: boolean;
  lavado: boolean;
  desinfeccion: boolean;
  desinsectacion: boolean;
  camaNueva: boolean;
  costo: number | null;
  estado: EstadoPreparacion;
  notas: string | null;
};

export type PreparacionInput = {
  galponId: string;
  loteAnteriorId?: string | null;
  fechaRetiro?: string | null;
  fechaInicio?: string;
  retiradaCama?: boolean;
  lavado?: boolean;
  desinfeccion?: boolean;
  desinsectacion?: boolean;
  camaNueva?: boolean;
  costo?: number | null;
  estado?: EstadoPreparacion;
  notas?: string | null;
};

export function searchPreparaciones(
  params: PagedParams & { galponId?: string; estado?: EstadoPreparacion } = {},
): Promise<PagedResponse<PreparacionDto>> {
  return apiFetch<PagedResponse<PreparacionDto>>(`${BASE}/preparaciones${query(params)}`);
}

export function getPreparacion(id: string): Promise<PreparacionDto> {
  return apiFetch<PreparacionDto>(`${BASE}/preparaciones/${encodeURIComponent(id)}`);
}

export function createPreparacion(input: PreparacionInput): Promise<string> {
  return apiFetch<string>(`${BASE}/preparaciones`, { method: "POST", body: JSON.stringify(input) });
}

export function updatePreparacion(id: string, input: PreparacionInput): Promise<string> {
  return apiFetch<string>(`${BASE}/preparaciones/${encodeURIComponent(id)}`, {
    method: "PUT",
    body: JSON.stringify(input),
  });
}

export async function deletePreparacion(id: string): Promise<void> {
  await apiFetch<void>(`${BASE}/preparaciones/${encodeURIComponent(id)}`, { method: "DELETE" });
}

export function completarPreparacion(id: string, fechaFin?: string | null): Promise<string> {
  return apiFetch<string>(`${BASE}/preparaciones/${encodeURIComponent(id)}/completar`, {
    method: "POST",
    body: JSON.stringify({ fechaFin: fechaFin ?? null }),
  });
}
