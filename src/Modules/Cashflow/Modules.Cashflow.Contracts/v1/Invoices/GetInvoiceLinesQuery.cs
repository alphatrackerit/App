using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>Returns the cash lines linked to an invoice plus the derived reconciliation totals (§9).</summary>
public sealed record GetInvoiceLinesQuery(Guid InvoiceId) : IQuery<InvoiceLinesDto>;
