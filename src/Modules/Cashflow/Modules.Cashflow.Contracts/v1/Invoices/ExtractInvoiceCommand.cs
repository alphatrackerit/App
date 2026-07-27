using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>
/// AI-extracted invoice data proposal. Every field is a SUGGESTION the user confirms in the form —
/// nothing is persisted by the extraction itself except the uploaded document.
/// <see cref="MatchedClientId"/>/<see cref="MatchedSupplierId"/> are set when
/// <see cref="CounterpartyName"/> resolves against the catalog (normalized match).
/// </summary>
public sealed record InvoiceExtractionDto(
    InvoiceType? Type,
    string? Number,
    string? DynamicsNumber,
    DateTime? InvoiceDate,
    DateTime? DueDate,
    decimal? TaxBase,
    decimal? Vat,
    decimal? Total,
    string? PaymentTerms,
    string? CounterpartyName,
    Guid? MatchedClientId,
    Guid? MatchedSupplierId,
    string? Bank,
    string? Notes,
    string DocumentPath);

/// <summary>
/// Stores the uploaded invoice document and asks the vision model for a data proposal (§ facturación
/// asistida por IA). The document is persisted regardless of extraction quality so the user can
/// attach it to the invoice they confirm.
/// </summary>
public sealed record ExtractInvoiceCommand(
    byte[] Content,
    string FileName,
    string ContentType) : ICommand<InvoiceExtractionDto>;
