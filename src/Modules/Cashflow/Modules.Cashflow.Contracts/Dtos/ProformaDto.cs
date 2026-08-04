using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Contracts.Dtos;

public sealed record ProformaDto(
    Guid Id,
    string Number,
    InvoiceType Type,
    DateTime? Date,
    Guid? ClientId,
    Guid? SupplierId,
    Guid? CompanyId,
    Guid? SocietyId,
    Guid? ProjectId,
    decimal? TaxBase,
    decimal? Vat,
    decimal Total,
    string? PaymentTerms,
    Guid? StatusId,
    string? Responsible,
    string? Notes,
    string? DocumentPath,
    decimal Invoiced);

/// <summary>One invoice generated from / linked to a proforma.</summary>
public sealed record ProformaInvoiceDto(
    Guid Id,
    string Number,
    DateTime? InvoiceDate,
    DateTime? DueDate,
    decimal Total,
    Guid? StatusId);

/// <summary>Reconciliation view of a proforma: its invoices plus derived totals. <c>Pending</c> =
/// <c>Total − Σ invoice totals</c> — informative only (may be negative = over-invoiced), never blocks.</summary>
public sealed record ProformaLinesDto(
    Guid ProformaId,
    decimal Total,
    decimal InvoicedAmount,
    decimal Pending,
    IReadOnlyList<ProformaInvoiceDto> Invoices);
