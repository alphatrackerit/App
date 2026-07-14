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
        public const string Confirm  = $"Permissions.{Resource}.Confirm";
        public const string Validate = $"Permissions.{Resource}.Validate";
    }

    public static class Payments
    {
        public const string Resource = "Projects.Payments";
        public const string View     = $"Permissions.{Resource}.View";
        public const string Create   = $"Permissions.{Resource}.Create";
        public const string Update   = $"Permissions.{Resource}.Update";
        public const string Delete   = $"Permissions.{Resource}.Delete";
        public const string Confirm  = $"Permissions.{Resource}.Confirm";
        public const string Validate = $"Permissions.{Resource}.Validate";
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
        new("Confirm Incomes",  "Confirm",              Incomes.Resource),
        new("Validate Incomes", "Validate",             Incomes.Resource),

        new("View Payments",     ActionConstants.View,   Payments.Resource, IsBasic: true),
        new("Create Payments",   ActionConstants.Create, Payments.Resource),
        new("Update Payments",   ActionConstants.Update, Payments.Resource),
        new("Delete Payments",   ActionConstants.Delete, Payments.Resource),
        new("Confirm Payments",  "Confirm",              Payments.Resource),
        new("Validate Payments", "Validate",             Payments.Resource),

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
    ];
}
