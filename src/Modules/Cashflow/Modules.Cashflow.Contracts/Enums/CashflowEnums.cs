namespace FSH.Modules.Cashflow.Contracts.Enums;

/// <summary>Discriminates a Status so one catalog serves Projects, Incomes, Payments and Invoices.</summary>
public enum StatusType
{
    Proyecto,
    Ingreso,
    Pago,
    FacturaEmitida,
    FacturaRecibida,
}

/// <summary>
/// Discriminates an <c>Invoice</c>: <c>Emitida</c> = issued to a client (drives Incomes),
/// <c>Recibida</c> = received from a supplier (drives Payments). Values are persisted as text
/// ("Emitida"/"Recibida") — do not renumber.
/// </summary>
public enum InvoiceType
{
    Emitida,
    Recibida,
}

/// <summary>A Prefix is either a top-level group or a category that belongs to a group.</summary>
public enum PrefixType
{
    Grupo,
    Categoria,
}

/// <summary>Which cash-line table a line belongs to: <c>Income</c> (Ingreso, drives Emitida invoices)
/// or <c>Payment</c> (Pago, drives Recibida invoices).</summary>
public enum CashLineKind
{
    Income,
    Payment,
}
