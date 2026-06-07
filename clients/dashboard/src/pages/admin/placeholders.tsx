import {
  BarChart3,
  Building2,
  Flag,
  ListChecks,
  Truck,
  Users,
  type LucideIcon,
} from "lucide-react";
import { clientsApi, companiesApi, countriesApi, statusesApi, suppliersApi } from "@/api/administration";
import { PageHero } from "@/components/list";
import { CatalogPage } from "./catalog-page";

// ── Real catalog pages (Administración) ─────────────────────────────────

export function ClientesPage() {
  return <CatalogPage title="Clientes" unit="cliente" icon={Users} queryKey="clients" api={clientsApi} description="Catálogo de clientes para asociar a los proyectos." />;
}

export function ProveedoresPage() {
  return <CatalogPage title="Proveedores" unit="proveedor" icon={Truck} queryKey="suppliers" api={suppliersApi} description="Catálogo de proveedores para asociar a los pagos." />;
}

export function PaisesPage() {
  return <CatalogPage title="Países" unit="país" icon={Flag} queryKey="countries" api={countriesApi} description="Catálogo de países." />;
}

export function EstadosPage() {
  return <CatalogPage title="Estados" unit="estado" icon={ListChecks} queryKey="statuses" api={statusesApi} description="Catálogo de estados para proyectos, ingresos y pagos." />;
}

export function EmpresasPage() {
  return <CatalogPage title="Empresas" unit="empresa" icon={Building2} queryKey="companies" api={companiesApi} description="Catálogo de empresas (sociedades)." />;
}

// ── Still-placeholder sections (no backend yet) ─────────────────────────

function ComingSoon({ icon: Icon, title, subtitle }: { icon: LucideIcon; title: string; subtitle: string }) {
  return (
    <div className="space-y-6">
      <PageHero eyebrow="Módulos" title={title} subtitle={subtitle} />
      <div className="flex flex-col items-center justify-center rounded-xl border border-dashed border-[var(--color-border)] bg-[var(--color-card)] py-20 text-center">
        <div className="mb-4 grid size-14 place-items-center rounded-2xl bg-[var(--color-muted)]">
          <Icon className="size-6 text-[oklch(from_var(--color-muted-foreground)_l_c_h_/_0.5)]" />
        </div>
        <h2 className="mb-1.5 font-display text-[17px] font-semibold text-[var(--color-foreground)]">Próximamente</h2>
        <p className="max-w-sm text-[13px] text-[var(--color-muted-foreground)]">
          Este módulo todavía no tiene backend. Se habilitará en una próxima iteración.
        </p>
      </div>
    </div>
  );
}

export function GraficosPage() {
  return <ComingSoon icon={BarChart3} title="Gráficos" subtitle="Informes y visualizaciones de rentabilidad." />;
}
