using FSH.Framework.Shared.Constants;

namespace FSH.Modules.Cashflow.Contracts.Authorization;

public static class CashflowPermissions
{
    public static class Projects
    {
        public const string Resource = "Projects.Projects";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Incomes
    {
        public const string Resource = "Projects.Incomes";
        public const string View     = $"Permissions.{Resource}.View";
        public const string Create   = $"Permissions.{Resource}.Create";
        public const string Update   = $"Permissions.{Resource}.Update";
        public const string Delete   = $"Permissions.{Resource}.Delete";
    }

    public static class Payments
    {
        public const string Resource = "Projects.Payments";
        public const string View     = $"Permissions.{Resource}.View";
        public const string Create   = $"Permissions.{Resource}.Create";
        public const string Update   = $"Permissions.{Resource}.Update";
        public const string Delete   = $"Permissions.{Resource}.Delete";
    }

    // Billing (facturación) — confirm/validate live under their own resource (spec §8).
    public static class Facturacion
    {
        public const string Resource = "Facturacion";
        public const string ConfirmIncome  = $"Permissions.{Resource}.ConfirmIncome";
        public const string ValidateIncome = $"Permissions.{Resource}.ValidateIncome";
        public const string ConfirmPago    = $"Permissions.{Resource}.ConfirmPago";
        public const string ValidatePago   = $"Permissions.{Resource}.ValidatePago";
    }

    public static class Notes
    {
        public const string Resource = "Projects.Notes";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    // ── Catalog master-data (consolidated from the former Administration module).
    // Resource strings are kept byte-identical ("Administration.*") — they are
    // persisted in role_claims and referenced by the dashboard nav gates. ──
    public static class Clients
    {
        public const string Resource = "Administration.Clients";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Suppliers
    {
        public const string Resource = "Administration.Suppliers";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Countries
    {
        public const string Resource = "Administration.Countries";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Statuses
    {
        public const string Resource = "Administration.Statuses";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Companies
    {
        public const string Resource = "Administration.Companies";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Societies
    {
        public const string Resource = "Administration.Societies";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Prefixes
    {
        public const string Resource = "Administration.Prefixes";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Projects",   ActionConstants.View,   Projects.Resource, IsBasic: true),
        new("Create Projects", ActionConstants.Create, Projects.Resource),
        new("Update Projects", ActionConstants.Update, Projects.Resource),
        new("Delete Projects", ActionConstants.Delete, Projects.Resource),

        new("View Incomes",     ActionConstants.View,   Incomes.Resource, IsBasic: true),
        new("Create Incomes",   ActionConstants.Create, Incomes.Resource),
        new("Update Incomes",   ActionConstants.Update, Incomes.Resource),
        new("Delete Incomes",   ActionConstants.Delete, Incomes.Resource),

        new("View Payments",     ActionConstants.View,   Payments.Resource, IsBasic: true),
        new("Create Payments",   ActionConstants.Create, Payments.Resource),
        new("Update Payments",   ActionConstants.Update, Payments.Resource),
        new("Delete Payments",   ActionConstants.Delete, Payments.Resource),

        new("Confirm income (billing)",   "ConfirmIncome",  Facturacion.Resource),
        new("Validate income (billing)",  "ValidateIncome", Facturacion.Resource),
        new("Confirm payment (billing)",  "ConfirmPago",    Facturacion.Resource),
        new("Validate payment (billing)", "ValidatePago",   Facturacion.Resource),

        new("View Notes",   ActionConstants.View,   Notes.Resource, IsBasic: true),
        new("Create Notes", ActionConstants.Create, Notes.Resource),
        new("Update Notes", ActionConstants.Update, Notes.Resource),
        new("Delete Notes", ActionConstants.Delete, Notes.Resource),

        new("View Clients",   ActionConstants.View,   Clients.Resource, IsBasic: true),
        new("Create Clients", ActionConstants.Create, Clients.Resource),
        new("Update Clients", ActionConstants.Update, Clients.Resource),
        new("Delete Clients", ActionConstants.Delete, Clients.Resource),

        new("View Suppliers",   ActionConstants.View,   Suppliers.Resource, IsBasic: true),
        new("Create Suppliers", ActionConstants.Create, Suppliers.Resource),
        new("Update Suppliers", ActionConstants.Update, Suppliers.Resource),
        new("Delete Suppliers", ActionConstants.Delete, Suppliers.Resource),

        new("View Countries",   ActionConstants.View,   Countries.Resource, IsBasic: true),
        new("Create Countries", ActionConstants.Create, Countries.Resource),
        new("Update Countries", ActionConstants.Update, Countries.Resource),
        new("Delete Countries", ActionConstants.Delete, Countries.Resource),

        new("View Statuses",   ActionConstants.View,   Statuses.Resource, IsBasic: true),
        new("Create Statuses", ActionConstants.Create, Statuses.Resource),
        new("Update Statuses", ActionConstants.Update, Statuses.Resource),
        new("Delete Statuses", ActionConstants.Delete, Statuses.Resource),

        new("View Companies",   ActionConstants.View,   Companies.Resource, IsBasic: true),
        new("Create Companies", ActionConstants.Create, Companies.Resource),
        new("Update Companies", ActionConstants.Update, Companies.Resource),
        new("Delete Companies", ActionConstants.Delete, Companies.Resource),

        new("View Societies",   ActionConstants.View,   Societies.Resource, IsBasic: true),
        new("Create Societies", ActionConstants.Create, Societies.Resource),
        new("Update Societies", ActionConstants.Update, Societies.Resource),
        new("Delete Societies", ActionConstants.Delete, Societies.Resource),

        new("View Prefixes",   ActionConstants.View,   Prefixes.Resource, IsBasic: true),
        new("Create Prefixes", ActionConstants.Create, Prefixes.Resource),
        new("Update Prefixes", ActionConstants.Update, Prefixes.Resource),
        new("Delete Prefixes", ActionConstants.Delete, Prefixes.Resource),
    ];
}
