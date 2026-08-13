using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Invoices;

/// <summary>
/// Creates an invoice. <see cref="Type"/> selects the factory: <c>Emitida</c> requires
/// <see cref="ClientId"/>, <c>Recibida</c> requires <see cref="SupplierId"/> (enforced by the
/// validator and the DB check constraint).
/// </summary>
public sealed record CreateInvoiceCommand(
    InvoiceType Type,
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
    string? DocumentPath = null,
    IReadOnlyList<InvoiceItemInput>? Items = null) : ICommand<Guid>;
