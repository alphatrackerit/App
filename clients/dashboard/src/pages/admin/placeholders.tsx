import { Building2, Flag, ListChecks, Truck, Users } from "lucide-react";
import { clientsApi, companiesApi, countriesApi, statusesApi, suppliersApi } from "@/api/administration";
import { CatalogPage } from "./catalog-page";

// Administración — catalog management pages, all backed by the Administration module.

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
