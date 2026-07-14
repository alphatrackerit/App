import { Building2, Flag, Users } from "lucide-react";
import { clientsApi, companiesApi, countriesApi } from "@/api/administration";
import { CatalogPage } from "./catalog-page";

// Administración — simple name+code catalog pages. Richer catalogs (Proveedores, Estados,
// Sociedades, Prefijos) live in ./rich-catalog.

export function ClientesPage() {
  return <CatalogPage title="Clientes" unit="cliente" icon={Users} queryKey="clients" api={clientsApi} description="Catálogo de clientes para asociar a los proyectos." />;
}

export function PaisesPage() {
  return <CatalogPage title="Países" unit="país" icon={Flag} queryKey="countries" api={countriesApi} description="Catálogo de países." />;
}

export function EmpresasPage() {
  return <CatalogPage title="Empresas" unit="empresa" icon={Building2} queryKey="companies" api={companiesApi} description="Catálogo de empresas." />;
}
