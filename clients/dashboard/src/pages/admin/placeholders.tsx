import {
  ArrowLeftRight,
  BarChart3,
  Building2,
  Flag,
  ListChecks,
  Truck,
  Users,
  type LucideIcon,
} from "lucide-react";
import { PageHero } from "@/components/list";

// Lightweight "coming soon" surface for AlphaTracker sections whose backend
// isn't built yet (Administración catalogs, Empresas, Flujo de caja, Gráficos).
// They live in the nav + routes so the app shell is complete; each renders a
// calm placeholder until its module ships.

function ComingSoon({
  icon: Icon,
  title,
  subtitle,
}: {
  icon: LucideIcon;
  title: string;
  subtitle: string;
}) {
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

export function ProveedoresPage() {
  return <ComingSoon icon={Truck} title="Proveedores" subtitle="Catálogo de proveedores para asociar a los pagos." />;
}

export function ClientesPage() {
  return <ComingSoon icon={Users} title="Clientes" subtitle="Catálogo de clientes para asociar a los proyectos." />;
}

export function PaisesPage() {
  return <ComingSoon icon={Flag} title="Países" subtitle="Catálogo de países." />;
}

export function EstadosPage() {
  return <ComingSoon icon={ListChecks} title="Estados" subtitle="Catálogo de estados para proyectos, ingresos y pagos." />;
}

export function EmpresasPage() {
  return <ComingSoon icon={Building2} title="Empresas" subtitle="Selecciona y administra las empresas (sociedades)." />;
}

export function FlujoDeCajaPage() {
  return <ComingSoon icon={ArrowLeftRight} title="Flujo de caja" subtitle="Ingresos y pagos consolidados por proyecto y periodo." />;
}

export function GraficosPage() {
  return <ComingSoon icon={BarChart3} title="Gráficos" subtitle="Informes y visualizaciones de rentabilidad." />;
}
