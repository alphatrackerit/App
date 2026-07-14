namespace FSH.Modules.Cashflow.Contracts.Enums;

/// <summary>Discriminates a Status so one catalog serves Projects, Incomes and Payments.</summary>
public enum StatusType
{
    Proyecto,
    Ingreso,
    Pago,
}

/// <summary>A Prefix is either a top-level group or a category that belongs to a group.</summary>
public enum PrefixType
{
    Grupo,
    Categoria,
}
