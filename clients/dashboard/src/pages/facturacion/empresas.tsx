import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Building2, Receipt } from "lucide-react";
import { companyCatalog } from "@/api/administration";
import { EntityPageHeader } from "@/components/list";

// Espejo de la pantalla "Empresas" de Proyectos, pero para Facturación: cada tarjeta
// filtra el listado de facturas por empresa; "Todas" lo muestra sin filtro. El alta y
// edición de empresas vive en Administración → Empresas.
export function EmpresasFacturacionPage() {
  const navigate = useNavigate();

  const q = useQuery({
    queryKey: ["administration", "companies", "cards-facturacion"],
    queryFn: () => companyCatalog.search({ pageSize: 10000, sortBy: "name", sortDir: "asc" }),
  });

  const items = q.data?.items ?? [];
  const cardBase =
    "group relative flex min-h-[110px] cursor-pointer flex-col items-center justify-center gap-2 rounded-xl border border-[var(--color-border)] bg-[var(--color-card)] p-4 text-center transition-all hover:-translate-y-0.5 hover:border-[var(--color-primary)] hover:shadow-md";

  return (
    <div className="space-y-4 sm:space-y-6">
      <EntityPageHeader
        icon={Building2}
        title="Empresas"
        total={q.data?.totalCount ?? null}
        unit="empresa"
        description="Seleccione una empresa para ver su facturación."
      />

      {q.isLoading ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {Array.from({ length: 6 }, (_, i) => (
            <div key={i} className="skeleton min-h-[110px] rounded-xl" aria-hidden />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          <button
            type="button"
            onClick={() => navigate("/facturacion/facturas")}
            aria-label="Ver facturas de todas las empresas"
            className={cardBase}
          >
            <Receipt className="size-6 text-[var(--color-primary)]" />
            <span className="text-[14px] font-semibold tracking-wide text-[var(--color-foreground)]">Todas</span>
            <span className="text-[12px] text-[var(--color-muted-foreground)]">Facturas de todas las empresas</span>
          </button>

          {items.map((it) => (
            <button
              key={it.id}
              type="button"
              onClick={() => navigate(`/facturacion/facturas?empresa=${encodeURIComponent(it.id)}`)}
              aria-label={`Ver facturas de ${it.name}`}
              className={cardBase}
            >
              <span className="text-[14px] font-semibold uppercase leading-snug text-[var(--color-foreground)] transition-colors group-hover:text-[var(--color-primary)]">
                {it.name}
              </span>
              {it.legalName && (
                <span className="truncate text-[12px] text-[var(--color-muted-foreground)]">{it.legalName}</span>
              )}
              {it.nif && (
                <span className="text-[11px] tabular-nums text-[var(--color-muted-foreground)]">{it.nif}</span>
              )}
            </button>
          ))}

          {items.length === 0 && (
            <p className="col-span-full rounded-xl border border-dashed border-[var(--color-border)] px-4 py-10 text-center text-[13px] text-[var(--color-muted-foreground)]">
              No hay empresas. Créalas en Administración → Empresas.
            </p>
          )}
        </div>
      )}
    </div>
  );
}
