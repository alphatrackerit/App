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

    // Billing (facturación) — invoice CRUD plus the confirm/validate line actions (spec §8).
    public static class Facturacion
    {
        public const string Resource = "Facturacion";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
        public const string ConfirmIncome  = $"Permissions.{Resource}.ConfirmIncome";
        public const string ValidateIncome = $"Permissions.{Resource}.ValidateIncome";
        public const string ConfirmPago    = $"Permissions.{Resource}.ConfirmPago";
        public const string ValidatePago   = $"Permissions.{Resource}.ValidatePago";
        // VERI*FACTU (AEAT) — issuing is irreversible; settings hold the company certificate.
        public const string IssueVerifactu           = $"Permissions.{Resource}.IssueVerifactu";
        public const string ManageVerifactuSettings  = $"Permissions.{Resource}.ManageVerifactuSettings";
    }

    // Proformas — commercial pre-invoice documents, one level above Invoice (1 proforma → N invoices).
    public static class Proformas
    {
        public const string Resource = "Proformas";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
        public const string GenerateInvoices = $"Permissions.{Resource}.GenerateInvoices";
    }

    public static class Reportes
    {
        public const string Resource = "Reportes";
        public const string View = $"Permissions.{Resource}.View";
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

    // Bank statement import (spec §33, optional).
    public static class Banks
    {
        public const string Resource = "Administration.Banks";
        public const string View   = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class BankMovements
    {
        public const string Resource = "Administration.BankMovements";
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

        new("View Invoices",   ActionConstants.View,   Facturacion.Resource, IsBasic: true),
        new("Create Invoices", ActionConstants.Create, Facturacion.Resource),
        new("Update Invoices", ActionConstants.Update, Facturacion.Resource),
        new("Delete Invoices", ActionConstants.Delete, Facturacion.Resource),

        new("Confirm income (billing)",   "ConfirmIncome",  Facturacion.Resource),
        new("Validate income (billing)",  "ValidateIncome", Facturacion.Resource),
        new("Confirm payment (billing)",  "ConfirmPago",    Facturacion.Resource),
        new("Validate payment (billing)", "ValidatePago",   Facturacion.Resource),
        new("Issue invoice under VERI*FACTU", "IssueVerifactu", Facturacion.Resource),
        new("Manage VeriFactu settings",  "ManageVerifactuSettings", Facturacion.Resource),

        new("View Proformas",   ActionConstants.View,   Proformas.Resource, IsBasic: true),
        new("Create Proformas", ActionConstants.Create, Proformas.Resource),
        new("Update Proformas", ActionConstants.Update, Proformas.Resource),
        new("Delete Proformas", ActionConstants.Delete, Proformas.Resource),
        new("Generate invoices from proforma", "GenerateInvoices", Proformas.Resource),

        new("View reports", ActionConstants.View, Reportes.Resource, IsBasic: true),

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

        new("View Banks",   ActionConstants.View,   Banks.Resource, IsBasic: true),
        new("Create Banks", ActionConstants.Create, Banks.Resource),
        new("Update Banks", ActionConstants.Update, Banks.Resource),
        new("Delete Banks", ActionConstants.Delete, Banks.Resource),

        new("View BankMovements",   ActionConstants.View,   BankMovements.Resource, IsBasic: true),
        new("Create BankMovements", ActionConstants.Create, BankMovements.Resource),
        new("Update BankMovements", ActionConstants.Update, BankMovements.Resource),
        new("Delete BankMovements", ActionConstants.Delete, BankMovements.Resource),
    ];
}
