using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>
/// Links an invoice to a proforma, or (with <see cref="ProformaId"/> = null) unlinks it. When
/// linking, the proforma's <c>Type</c> must match the invoice's (Emitida↔Emitida, Recibida↔Recibida).
/// No amount validation — the cuadre is informative only.
/// </summary>
public sealed record LinkInvoiceToProformaCommand(
    Guid InvoiceId,
    Guid? ProformaId) : ICommand<Unit>;
