using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>Marks the invoice "COMPROBADO CON LISTADO" (reconciled against the master listing).</summary>
public sealed record MarkInvoiceVerifiedCommand(Guid InvoiceId) : ICommand<Unit>;
