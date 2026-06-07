using FSH.Framework.Shared.Constants;

namespace FSH.Modules.Administration.Contracts.Authorization;

public static class AdministrationPermissions
{
    public static class Clients
    {
        public const string Resource = "Administration.Clients";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Suppliers
    {
        public const string Resource = "Administration.Suppliers";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Countries
    {
        public const string Resource = "Administration.Countries";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Statuses
    {
        public const string Resource = "Administration.Statuses";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static class Companies
    {
        public const string Resource = "Administration.Companies";
        public const string View = $"Permissions.{Resource}.View";
        public const string Create = $"Permissions.{Resource}.Create";
        public const string Update = $"Permissions.{Resource}.Update";
        public const string Delete = $"Permissions.{Resource}.Delete";
    }

    public static IReadOnlyList<FshPermission> All { get; } =
    [
        new("View Clients", ActionConstants.View, Clients.Resource, IsBasic: true),
        new("Create Clients", ActionConstants.Create, Clients.Resource),
        new("Update Clients", ActionConstants.Update, Clients.Resource),
        new("Delete Clients", ActionConstants.Delete, Clients.Resource),

        new("View Suppliers", ActionConstants.View, Suppliers.Resource, IsBasic: true),
        new("Create Suppliers", ActionConstants.Create, Suppliers.Resource),
        new("Update Suppliers", ActionConstants.Update, Suppliers.Resource),
        new("Delete Suppliers", ActionConstants.Delete, Suppliers.Resource),

        new("View Countries", ActionConstants.View, Countries.Resource, IsBasic: true),
        new("Create Countries", ActionConstants.Create, Countries.Resource),
        new("Update Countries", ActionConstants.Update, Countries.Resource),
        new("Delete Countries", ActionConstants.Delete, Countries.Resource),

        new("View Statuses", ActionConstants.View, Statuses.Resource, IsBasic: true),
        new("Create Statuses", ActionConstants.Create, Statuses.Resource),
        new("Update Statuses", ActionConstants.Update, Statuses.Resource),
        new("Delete Statuses", ActionConstants.Delete, Statuses.Resource),

        new("View Companies", ActionConstants.View, Companies.Resource, IsBasic: true),
        new("Create Companies", ActionConstants.Create, Companies.Resource),
        new("Update Companies", ActionConstants.Update, Companies.Resource),
        new("Delete Companies", ActionConstants.Delete, Companies.Resource),
    ];
}
