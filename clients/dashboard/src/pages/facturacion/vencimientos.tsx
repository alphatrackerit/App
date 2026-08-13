import { useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { CalendarClock } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { searchProjects } from "@/api/projects";
import { clientsApi, companiesApi, suppliersApi } from "@/api/administration";
import {
  Combobox,
  EntityColHeader,
  EntityFilterPill,
  EntityListCard,
  EntityListHeader,
  EntityPageHeader,
  EntityStatusBadge,
  useTableControls,
} from "@/components/list";
import { describe } from "@/lib/list-helpers";
import { EPS, fmtDia, fmtEur, useFacturacion, type Vencimiento, type VencimientoEstado } from "./data";
import { EstadoVencimientoBadge, useRegistrar } from "./shared";

const COLS =
  "grid grid-cols-[minmax(0,1fr)_84px_minmax(0,1.2fr)_100px_60px_92px_105px_105px_105px_90px_88px] items-center gap-2";

type EstadoFilter = VencimientoEstado | null;

export function VencimientosPage() {
  const { vencimientos, isLoading, isError, error } = useFacturacion();
  const registrar = useRegistrar();

  // ── Filtros ──
  const [supplierId, setSupplierId] = useState<string | null>(null);
  const [clientId, setClientId] = useState<string | null>(null);
  const [companyId, setCompanyId] = useState<string | null>(null);
  const [projectId, setProjectId] = useState<string | null>(null);
  const [estado, setEstado] = useState<EstadoFilter>(null);
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");

  const clientsQ = useQuery({
    queryKey: ["administration", "clients", "options"],
    queryFn: () => clientsApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    staleTime: 5 * 60_000,
  });
  const suppliersQ = useQuery({
    queryKey: ["administration", "suppliers", "options"],
    queryFn: () => suppliersApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    staleTime: 5 * 60_000,
  });
  const companiesQ = useQuery({
    queryKey: ["administration", "companies", "options"],
    queryFn: () => companiesApi.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
    staleTime: 5 * 60_000,
  });
  const projectsQ = useQuery({
    queryKey: ["projects", "options"],
    queryFn: () => searchProjects({ pageSize: 10000 }),
    staleTime: 5 * 60_000,
  });

  const filtered = useMemo(
    () =>
      vencimientos.filter((v) => {
        if (supplierId && v.supplierId !== supplierId) return false;
        if (clientId && v.clientId !== clientId) return false;
        if (companyId && v.companyId !== companyId) return false;
        if (projectId && v.projectId !== projectId) return false;
        if (estado && v.estado !== estado) return false;
        if (from && (!v.dueDate || v.dueDate.slice(0, 10) < from)) return false;
        if (to && (!v.dueDate || v.dueDate.slice(0, 10) > to)) return false;
        return true;
      }),
    [vencimientos, supplierId, clientId, companyId, projectId, estado, from, to],
  );

  const ctl = useTableControls<Vencimiento>(filtered, {
    referencia: (v) => v.reference,
    tipo: (v) => v.kind,
    contraparte: (v) => v.counterparty,
    pago: (v) => v.seq,
    porcentaje: (v) => v.percentage,
    vence: (v) => v.dueDate,
    importe: (v) => v.amount,
    pagado: (v) => v.paid,
    pendiente: (v) => v.pending,
    estado: (v) => v.estado,
  });

  const hasFilters =
    supplierId !== null || clientId !== null || companyId !== null || projectId !== null || estado !== null || !!from || !!to;

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={CalendarClock}
        title="Vencimientos"
        total={filtered.length || null}
        unit="vencimiento"
        description="Esta es la deuda real: el aging se lee de aquí, no de las facturas. Los hitos «Previsto» se derivan de la forma de pago."
      />

      <div className="flex flex-wrap items-center gap-2">
        <Combobox
          label="Proveedor"
          value={supplierId}
          onChange={setSupplierId}
          options={(suppliersQ.data?.items ?? []).map((s) => ({ value: s.id, label: s.name }))}
          variant="filter"
          searchable
          clearable
        />
        <Combobox
          label="Cliente"
          value={clientId}
          onChange={setClientId}
          options={(clientsQ.data?.items ?? []).map((c) => ({ value: c.id, label: c.name }))}
          variant="filter"
          searchable
          clearable
        />
        <Combobox
          label="Empresa"
          value={companyId}
          onChange={setCompanyId}
          options={(companiesQ.data?.items ?? []).map((c) => ({ value: c.id, label: c.name }))}
          variant="filter"
          searchable
          clearable
        />
        <Combobox
          label="Proyecto"
          value={projectId}
          onChange={setProjectId}
          options={(projectsQ.data?.items ?? []).map((p) => ({ value: p.id, label: p.name }))}
          variant="filter"
          searchable
          clearable
        />
        <EntityFilterPill<EstadoFilter>
          label="Estado"
          value={estado}
          onChange={setEstado}
          options={[
            { value: null, label: "Todos" },
            { value: "Abierto", label: "Abierto" },
            { value: "Vencido", label: "Vencido" },
            { value: "Pagada", label: "Pagado" },
            { value: "Previsto", label: "Previsto" },
          ]}
        />
        <div className="flex items-center gap-1.5">
          <Input
            type="date"
            value={from}
            onChange={(e) => setFrom(e.target.value)}
            aria-label="Vence desde"
            className="h-8 w-[138px] text-[12.5px]"
          />
          <span className="text-[12px] text-[var(--color-muted-foreground)]">–</span>
          <Input
            type="date"
            value={to}
            onChange={(e) => setTo(e.target.value)}
            aria-label="Vence hasta"
            className="h-8 w-[138px] text-[12.5px]"
          />
        </div>
      </div>

      {isError ? (
        <div role="alert" className="rounded-lg border border-[oklch(from_var(--color-destructive)_l_c_h_/_0.30)] bg-[oklch(from_var(--color-destructive)_l_c_h_/_0.06)] px-3 py-2 text-sm text-[var(--color-destructive)]">
          {describe(error)}
        </div>
      ) : isLoading ? (
        <p className="rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          Cargando vencimientos…
        </p>
      ) : filtered.length === 0 ? (
        <p className="rounded-xl border border-dashed border-[var(--color-border)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
          {hasFilters
            ? "Ningún vencimiento coincide con los filtros."
            : "Aún no hay vencimientos. Se crean vinculando líneas de caja a una factura (o generando sus hitos)."}
        </p>
      ) : (
        <EntityListCard>
          <EntityListHeader className={COLS}>
            {(
              [
                ["referencia", "Referencia", undefined],
                ["tipo", "Tipo", undefined],
                ["contraparte", "Contraparte", undefined],
                ["pago", "Pago", undefined],
                ["porcentaje", "%", "right"],
                ["vence", "Vence", undefined],
                ["importe", "Importe", "right"],
                ["pagado", "Pagado", "right"],
                ["pendiente", "Pendiente", "right"],
                ["estado", "Estado", undefined],
              ] as const
            ).map(([key, label, align]) => (
              <EntityColHeader
                key={key}
                colKey={key}
                label={label}
                align={align}
                sort={ctl.sort}
                filters={ctl.filters}
                onSort={ctl.toggleSort}
                onFilter={ctl.setFilter}
              />
            ))}
            <span />
          </EntityListHeader>
          <ul className="divide-y divide-[oklch(from_var(--color-border)_l_c_h_/_0.5)]">
            {ctl.rows.map((v) => (
              <li key={v.id} className={`${COLS} px-5 py-2.5`}>
                <span
                  className={`truncate font-mono text-[12px] ${v.invoiceNumber ? "text-[var(--color-primary)]" : "italic text-[var(--color-muted-foreground)]"}`}
                  title={v.reference}
                >
                  {v.reference}
                </span>
                <span>
                  <EntityStatusBadge tone={v.kind === "cobro" ? "info" : "default"}>{v.kind}</EntityStatusBadge>
                </span>
                <span className="truncate text-[13px] text-[var(--color-foreground)]" title={v.counterparty}>
                  {v.counterparty}
                </span>
                <span className="text-[12.5px] text-[var(--color-muted-foreground)]">
                  Pago {v.seq}/{v.seqTotal}
                </span>
                <span className="text-right text-[12.5px] tabular-nums text-[var(--color-muted-foreground)]">
                  {v.percentage != null ? `${v.percentage} %` : "—"}
                </span>
                <span className={`text-[12.5px] tabular-nums ${v.estado === "Vencido" ? "text-[var(--color-destructive)]" : "text-[var(--color-muted-foreground)]"}`}>
                  {fmtDia(v.dueDate)}
                </span>
                <span className="text-right text-[13px] tabular-nums text-[var(--color-foreground)]">{fmtEur(v.amount)}</span>
                <span className="text-right text-[13px] tabular-nums text-[var(--color-muted-foreground)]">{fmtEur(v.paid)}</span>
                <span className="text-right text-[13px] font-medium tabular-nums text-[var(--color-foreground)]">{fmtEur(v.pending)}</span>
                <span>
                  <EstadoVencimientoBadge estado={v.estado} />
                </span>
                <span className="flex justify-end">
                  {/* Un hito previsto no tiene línea de caja: se registra generando los hitos de la factura. */}
                  {!v.previsto && v.pending > EPS && (
                    <Button
                      variant="outline"
                      disabled={registrar.isPending}
                      onClick={() => registrar.mutate({ id: v.id, kind: v.kind })}
                      className="h-7 rounded-md px-2.5 text-[12px]"
                    >
                      Registrar
                    </Button>
                  )}
                </span>
              </li>
            ))}
          </ul>
        </EntityListCard>
      )}
    </div>
  );
}
