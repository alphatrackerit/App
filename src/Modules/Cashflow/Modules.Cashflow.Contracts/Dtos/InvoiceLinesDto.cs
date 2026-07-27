using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Contracts.Dtos;

/// <summary>One cash line (Income or Payment) attached to an invoice.
/// Carries every editable field so the client can round-trip an update without wiping data.</summary>
public sealed record InvoiceLineDto(
    Guid Id,
    CashLineKind Kind,
    decimal Amount,
    DateTime? Date,
    decimal? Percentage,
    string? Description,
    Guid? ProjectId,
    Guid? StatusId,
    Guid? SupplierId,
    bool Confirmed,
    bool Validated);

/// <summary>Reconciliation view of an invoice (§9): its lines plus the derived totals.
/// <c>Pending</c> = <c>Total − Σ validated line amounts</c> — never persisted, always derived.</summary>
public sealed record InvoiceLinesDto(
    Guid InvoiceId,
    decimal Total,
    decimal LinkedAmount,
    decimal ValidatedAmount,
    decimal Pending,
    IReadOnlyList<InvoiceLineDto> Lines);
