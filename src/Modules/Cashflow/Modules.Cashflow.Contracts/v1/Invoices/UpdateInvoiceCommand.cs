using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>Updates an invoice's editable header fields. The invoice <c>Type</c> is immutable.</summary>
public sealed record UpdateInvoiceCommand(
    Guid InvoiceId,
    string? Number,
    decimal Total,
    Guid? ClientId = null,
    Guid? SupplierId = null,
    DateTime? InvoiceDate = null,
    DateTime? DueDate = null,
    Guid? CompanyId = null,
    Guid? SocietyId = null,
    Guid? ProjectId = null,
    decimal? TaxBase = null,
    decimal? Vat = null,
    string? PaymentTerms = null,
    string? Bank = null,
    Guid? StatusId = null,
    string? DynamicsNumber = null,
    string? Notes = null,
    IReadOnlyList<InvoiceItemInput>? Items = null) : ICommand<Guid>;
