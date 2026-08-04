namespace FSH.Modules.Cashflow.Contracts.Enums;

/// <summary>Discriminates a Status so one catalog serves Projects, Incomes, Payments, Invoices and Proformas.</summary>
public enum StatusType
{
    Proyecto,
    Ingreso,
    Pago,
    FacturaEmitida,
    FacturaRecibida,
    ProformaEmitida,
    ProformaRecibida,
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

/// <summary>
/// VERI*FACTU state of an invoice's AEAT billing record. English type name + Spanish values, same
/// convention as <see cref="InvoiceType"/>. Persisted as text — do not renumber. Distinct from the
/// internal "Verificada" check (<c>Invoice.Verified</c>/<c>MarkInvoiceVerified</c>) — never mix them.
/// </summary>
public enum VerifactuStatus
{
    /// <summary>Not subject to VeriFactu (received invoices, or issued before the engine).</summary>
    NoAplica,

    /// <summary>Chained record generated locally, waiting to be sent to the AEAT.</summary>
    PendienteEnvio,

    /// <summary>Sent to the AEAT, no response processed yet.</summary>
    Enviada,

    /// <summary>Accepted by the AEAT.</summary>
    Aceptada,

    /// <summary>Accepted by the AEAT with (non-blocking) errors.</summary>
    AceptadaConErrores,

    /// <summary>Rejected by the AEAT — compliance incident, must be surfaced.</summary>
    Rechazada,

    /// <summary>Transport/technical failure — retried by the sweep job.</summary>
    ErrorTecnico,
}

/// <summary>AEAT target environment for a company's VeriFactu submissions.</summary>
public enum VerifactuEnvironment
{
    Pruebas,
    Produccion,
}
