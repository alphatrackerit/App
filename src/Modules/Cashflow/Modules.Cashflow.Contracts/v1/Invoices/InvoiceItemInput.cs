namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>One concept line sent on create/update. The stored Amount is derived
/// (Quantity × UnitPrice) — never client-supplied.</summary>
public sealed record InvoiceItemInput(string Description, decimal Quantity, decimal UnitPrice);
