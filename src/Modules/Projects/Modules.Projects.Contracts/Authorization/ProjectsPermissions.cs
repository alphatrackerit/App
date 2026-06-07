using FSH.Framework.Shared.Constants;

namespace FSH.Modules.Projects.Contracts.Authorization;

public static class ProjectsPermissions
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
    ];
}
