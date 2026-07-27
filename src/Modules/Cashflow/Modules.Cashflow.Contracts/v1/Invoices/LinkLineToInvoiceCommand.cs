using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>
/// Links a cash line to an invoice, or (with <see cref="InvoiceId"/> = null) unlinks it. The
/// <see cref="Kind"/> selects the line table; when linking, the invoice's <c>Type</c> must match
/// (Income↔Emitida, Payment↔Recibida).
/// </summary>
public sealed record LinkLineToInvoiceCommand(
    Guid LineId,
    CashLineKind Kind,
    Guid? InvoiceId) : ICommand<Unit>;
