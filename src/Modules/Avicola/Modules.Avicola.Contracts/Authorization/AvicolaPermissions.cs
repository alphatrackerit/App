using FSH.Framework.Shared.Constants;

namespace FSH.Modules.Avicola.Contracts.Authorization;

/// <summary>
/// Avícola permissions. Segmentation is by the <c>IsBasic</c> flag:
/// <list type="bullet">
/// <item><b>Operario</b> (Basic role / morning data collection): View everything + <b>Create</b> on
/// the daily records (mortalidad, alimentación, pesos, sanidad) and on documents.</item>
/// <item><b>Administración</b> (Admin role): everything — flocks/sheds CRUD, dispatches, orders,
/// accounting movements, document deletion, settlement and the accounting reports.</item>
/// </list>
/// </summary>
public static class AvicolaPermissions
{
    public static class Galpones
    {
        public const string Resource = "Avicola.Galpones";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Lotes
    {
        public const string Resource = "Avicola.Lotes";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
        public const string Cerrar = $"Permissions.{Resource}.Cerrar";
    }

    public static class Mortalidad
    {
        public const string Resource = "Avicola.Mortalidad";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Alimentacion
    {
        public const string Resource = "Avicola.Alimentacion";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Pesos
    {
        public const string Resource = "Avicola.Pesos";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Sanidad
    {
        public const string Resource = "Avicola.Sanidad";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Despachos
    {
        public const string Resource = "Avicola.Despachos";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Documentos
    {
        public const string Resource = "Avicola.Documentos";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Pedidos
    {
        public const string Resource = "Avicola.Pedidos";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Movimientos
    {
        public const string Resource = "Avicola.Movimientos";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Contabilidad
    {
        public const string Resource = "Avicola.Contabilidad";
        public const string View = $"Permissions.{Resource}.View";
    }

    public static class Preparaciones
    {
        public const string Resource = "Avicola.Preparaciones";
        public const string View     = $"Permissions.{Resource}.View";
        public const string Create   = $"Permissions.{Resource}.Create";
        public const string Update   = $"Permissions.{Resource}.Update";
        public const string Delete   = $"Permissions.{Resource}.Delete";
        public const string Completar = $"Permissions.{Resource}.Completar";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        // ── Galpones (sheds) — view basic, edit admin ──
        new("View Galpones",   ActionConstants.View,   Galpones.Resource, IsBasic: true),
        new("Create Galpones", ActionConstants.Create, Galpones.Resource),
        new("Update Galpones", ActionConstants.Update, Galpones.Resource),
        new("Delete Galpones", ActionConstants.Delete, Galpones.Resource),

        // ── Lotes (flocks) — view basic, edit/close admin ──
        new("View Lotes",   ActionConstants.View,   Lotes.Resource, IsBasic: true),
        new("Create Lotes", ActionConstants.Create, Lotes.Resource),
        new("Update Lotes", ActionConstants.Update, Lotes.Resource),
        new("Delete Lotes", ActionConstants.Delete, Lotes.Resource),
        new("Cerrar Lotes", "Cerrar",               Lotes.Resource),

        // ── Daily records — view + CREATE are basic (operario morning collection); edit/delete admin ──
        new("View Mortalidad",   ActionConstants.View,   Mortalidad.Resource, IsBasic: true),
        new("Create Mortalidad", ActionConstants.Create, Mortalidad.Resource, IsBasic: true),
        new("Update Mortalidad", ActionConstants.Update, Mortalidad.Resource),
        new("Delete Mortalidad", ActionConstants.Delete, Mortalidad.Resource),

        new("View Alimentacion",   ActionConstants.View,   Alimentacion.Resource, IsBasic: true),
        new("Create Alimentacion", ActionConstants.Create, Alimentacion.Resource, IsBasic: true),
        new("Update Alimentacion", ActionConstants.Update, Alimentacion.Resource),
        new("Delete Alimentacion", ActionConstants.Delete, Alimentacion.Resource),

        new("View Pesos",   ActionConstants.View,   Pesos.Resource, IsBasic: true),
        new("Create Pesos", ActionConstants.Create, Pesos.Resource, IsBasic: true),
        new("Update Pesos", ActionConstants.Update, Pesos.Resource),
        new("Delete Pesos", ActionConstants.Delete, Pesos.Resource),

        new("View Sanidad",   ActionConstants.View,   Sanidad.Resource, IsBasic: true),
        new("Create Sanidad", ActionConstants.Create, Sanidad.Resource, IsBasic: true),
        new("Update Sanidad", ActionConstants.Update, Sanidad.Resource),
        new("Delete Sanidad", ActionConstants.Delete, Sanidad.Resource),

        // ── Despachos (harvest / sale) — view basic, write admin ──
        new("View Despachos",   ActionConstants.View,   Despachos.Resource, IsBasic: true),
        new("Create Despachos", ActionConstants.Create, Despachos.Resource),
        new("Update Despachos", ActionConstants.Update, Despachos.Resource),
        new("Delete Despachos", ActionConstants.Delete, Despachos.Resource),

        // ── Documentos — view + attach basic (operario adjunta albaranes/partes); delete admin ──
        new("View Documentos",   ActionConstants.View,   Documentos.Resource, IsBasic: true),
        new("Create Documentos", ActionConstants.Create, Documentos.Resource, IsBasic: true),
        new("Delete Documentos", ActionConstants.Delete, Documentos.Resource),

        // ── Pedidos (purchase orders) — admin only ──
        new("View Pedidos",   ActionConstants.View,   Pedidos.Resource),
        new("Create Pedidos", ActionConstants.Create, Pedidos.Resource),
        new("Update Pedidos", ActionConstants.Update, Pedidos.Resource),
        new("Delete Pedidos", ActionConstants.Delete, Pedidos.Resource),

        // ── Movimientos contables (manual ledger) — admin only ──
        new("View Movimientos",   ActionConstants.View,   Movimientos.Resource),
        new("Create Movimientos", ActionConstants.Create, Movimientos.Resource),
        new("Update Movimientos", ActionConstants.Update, Movimientos.Resource),
        new("Delete Movimientos", ActionConstants.Delete, Movimientos.Resource),

        // ── Contabilidad (accounting + settlement reports) — admin only ──
        new("View Contabilidad", ActionConstants.View, Contabilidad.Resource),

        // ── Preparación de nave (vacío sanitario) — view/record basic; delete admin ──
        new("View Preparaciones",     ActionConstants.View,   Preparaciones.Resource, IsBasic: true),
        new("Create Preparaciones",   ActionConstants.Create, Preparaciones.Resource, IsBasic: true),
        new("Update Preparaciones",   ActionConstants.Update, Preparaciones.Resource, IsBasic: true),
        new("Completar Preparaciones", "Completar",           Preparaciones.Resource, IsBasic: true),
        new("Delete Preparaciones",   ActionConstants.Delete, Preparaciones.Resource),
    ];
}
