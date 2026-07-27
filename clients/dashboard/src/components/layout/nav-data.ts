import {
  Activity,
  ArrowLeftRight,
  BarChart3,
  Banknote,
  Bird,
  Briefcase,
  CalendarClock,
  Send,
  Building2,
  Calculator,
  CreditCard,
  FileText,
  Flag,
  Landmark,
  FolderOpen,
  FolderTree,
  HeartPulse,
  LayoutDashboard,
  ListChecks,
  MessageCircle,
  Package,
  Receipt,
  ScrollText,
  Settings,
  ShieldCheck,
  ShoppingCart,
  Sparkles,
  Tags,
  Ticket,
  Trash2,
  Truck,
  Users,
  UsersRound,
  Warehouse,
  Wifi,
  Wrench,
} from "lucide-react";

export type NavSpec = {
  to: string;
  label: string;
  icon: React.ComponentType<{ className?: string }>;
  /**
   * Permission required to see this item. Items without a `perm` are visible to
   * every authenticated tenant user; gated items are hidden when the current
   * user (or impersonated user) lacks the permission, so they never land on a
   * page the API will reject with 403.
   */
  perm?: string;
};

export type NavSection = {
  id: string;
  caption: string;
  /** Section-level icon used as a fallback when the sidebar is
   *  collapsed and the section is rendered as a stack of item icons. */
  icon: React.ComponentType<{ className?: string }>;
  items: NavSpec[];
};

// Top-level items live OUTSIDE any section. Overview opens the app;
// Settings is account-scoped and lives at the very bottom.
export const topNavTop: NavSpec[] = [
  { to: "/", label: "Overview", icon: LayoutDashboard },
  { to: "/chat", label: "Chat", icon: MessageCircle },
  { to: "/files", label: "My Files", icon: FolderOpen },
];

export const topNavBottom: NavSpec[] = [
  { to: "/settings", label: "Settings", icon: Settings },
];

// Section accordion. Single-select — only one section open at a time.
export const sections: NavSection[] = [
  {
    id: "proyectos",
    caption: "Proyectos",
    icon: Briefcase,
    items: [
      { to: "/projects", label: "Proyectos", icon: Briefcase, perm: "Permissions.Projects.Projects.View" },
      { to: "/flujo-de-caja", label: "Flujo de caja", icon: ArrowLeftRight, perm: "Permissions.Projects.Projects.View" },
      { to: "/graficos", label: "Gráficos", icon: BarChart3, perm: "Permissions.Projects.Projects.View" },
      { to: "/empresas", label: "Empresas", icon: Building2, perm: "Permissions.Administration.Companies.View" },
      { to: "/log", label: "Log", icon: ScrollText, perm: "Permissions.AuditTrails.View" },
    ],
  },
  {
    id: "facturacion",
    caption: "Facturación",
    icon: Receipt,
    items: [
      { to: "/facturacion", label: "Panel", icon: LayoutDashboard, perm: "Permissions.Facturacion.View" },
      { to: "/facturacion/facturas", label: "Facturas", icon: FileText, perm: "Permissions.Facturacion.View" },
      { to: "/facturacion/vencimientos", label: "Vencimientos", icon: CalendarClock, perm: "Permissions.Facturacion.View" },
      { to: "/facturacion/pagos-cobros", label: "Pagos y cobros", icon: Banknote, perm: "Permissions.Facturacion.View" },
      { to: "/facturacion/remesas", label: "Remesas", icon: Send, perm: "Permissions.Facturacion.View" },
    ],
  },
  {
    id: "avicola",
    caption: "Avícola",
    icon: Bird,
    items: [
      { to: "/avicola", label: "Panel", icon: LayoutDashboard, perm: "Permissions.Avicola.Lotes.View" },
      { to: "/avicola/lotes", label: "Lotes", icon: Bird, perm: "Permissions.Avicola.Lotes.View" },
      { to: "/avicola/galpones", label: "Galpones", icon: Warehouse, perm: "Permissions.Avicola.Galpones.View" },
      { to: "/avicola/preparaciones", label: "Preparación", icon: Sparkles, perm: "Permissions.Avicola.Preparaciones.View" },
      { to: "/avicola/pedidos", label: "Pedidos", icon: ShoppingCart, perm: "Permissions.Avicola.Pedidos.View" },
      { to: "/avicola/contabilidad", label: "Contabilidad", icon: Calculator, perm: "Permissions.Avicola.Contabilidad.View" },
    ],
  },
  {
    id: "administracion",
    caption: "Administración",
    icon: Wrench,
    items: [
      { to: "/admin/proveedores", label: "Proveedores", icon: Truck, perm: "Permissions.Administration.Suppliers.View" },
      { to: "/admin/clientes", label: "Clientes", icon: Users, perm: "Permissions.Administration.Clients.View" },
      { to: "/admin/paises", label: "Países", icon: Flag, perm: "Permissions.Administration.Countries.View" },
      { to: "/admin/estados", label: "Estados", icon: ListChecks, perm: "Permissions.Administration.Statuses.View" },
      { to: "/admin/empresas", label: "Empresas", icon: Building2, perm: "Permissions.Administration.Companies.View" },
      { to: "/admin/sociedades", label: "Sociedades", icon: Building2, perm: "Permissions.Administration.Societies.View" },
      { to: "/admin/prefijos", label: "Prefijos", icon: Tags, perm: "Permissions.Administration.Prefixes.View" },
      { to: "/admin/bancos", label: "Bancos", icon: Landmark, perm: "Permissions.Administration.Banks.View" },
      { to: "/admin/movimientos-banco", label: "Movimientos", icon: Banknote, perm: "Permissions.Administration.BankMovements.View" },
    ],
  },
  {
    id: "operations",
    caption: "Operations",
    icon: Activity,
    items: [
      { to: "/activity", label: "Live activity", icon: Activity },
      { to: "/subscription", label: "Subscription", icon: CreditCard, perm: "Permissions.Billing.View" },
      { to: "/invoices", label: "Invoices", icon: Receipt, perm: "Permissions.Billing.View" },
    ],
  },
  {
    id: "catalog",
    caption: "Catalog",
    icon: Package,
    items: [
      { to: "/catalog/products", label: "Products", icon: Package, perm: "Permissions.Catalog.Products.View" },
      { to: "/catalog/brands", label: "Brands", icon: Tags, perm: "Permissions.Catalog.Brands.View" },
      { to: "/catalog/categories", label: "Categories", icon: FolderTree, perm: "Permissions.Catalog.Categories.View" },
    ],
  },
  {
    id: "helpdesk",
    caption: "Helpdesk",
    icon: Ticket,
    items: [
      { to: "/tickets", label: "Tickets", icon: Ticket, perm: "Permissions.Tickets.View" },
    ],
  },
  {
    id: "identity",
    caption: "Identity",
    icon: Users,
    items: [
      { to: "/identity/users", label: "Users", icon: Users, perm: "Permissions.Users.View" },
      { to: "/identity/roles", label: "Roles", icon: ShieldCheck, perm: "Permissions.Roles.View" },
      { to: "/identity/groups", label: "Groups", icon: UsersRound, perm: "Permissions.Groups.View" },
    ],
  },
  {
    id: "system",
    caption: "System",
    icon: HeartPulse,
    items: [
      { to: "/system/health", label: "Health", icon: HeartPulse },
      { to: "/system/audits", label: "Audit trail", icon: ScrollText, perm: "Permissions.AuditTrails.View" },
      { to: "/system/sessions", label: "Sessions", icon: Wifi, perm: "Permissions.Sessions.ViewAll" },
      { to: "/system/trash", label: "Trash", icon: Trash2, perm: "Permissions.Catalog.Products.Restore" },
    ],
  },
];

/** True when the item is ungated, or the user holds its required permission. */
function isNavItemVisible(item: NavSpec, permissions: readonly string[]): boolean {
  return !item.perm || permissions.includes(item.perm);
}

/** Drop items the user can't access, then drop any section left empty. */
export function visibleSections(permissions: readonly string[]): NavSection[] {
  return sections
    .map((s) => ({ ...s, items: s.items.filter((i) => isNavItemVisible(i, permissions)) }))
    .filter((s) => s.items.length > 0);
}

/** Filter a flat nav list (top/bottom) by permission. */
export function visibleItems(items: NavSpec[], permissions: readonly string[]): NavSpec[] {
  return items.filter((i) => isNavItemVisible(i, permissions));
}

/** Find the section whose items contain the given path (best prefix match). */
export function findSectionForPath(pathname: string): string | null {
  let bestId: string | null = null;
  let bestLen = 0;
  for (const s of sections) {
    for (const item of s.items) {
      if (
        (item.to === "/" && pathname === "/") ||
        (item.to !== "/" && pathname.startsWith(item.to))
      ) {
        if (item.to.length > bestLen) {
          bestLen = item.to.length;
          bestId = s.id;
        }
      }
    }
  }
  return bestId;
}
