using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>
/// Creates a proforma. <see cref="Type"/> selects the factory (reusing <see cref="InvoiceType"/> —
/// same semantics as invoices): <c>Emitida</c> requires <see cref="ClientId"/>, <c>Recibida</c>
/// requires <see cref="SupplierId"/> (enforced by the validator and the DB check constraint).
/// </summary>
public sealed record CreateProformaCommand(
    InvoiceType Type,
    string Number,
    decimal Total,
    Guid? ClientId = null,
    Guid? SupplierId = null,
    DateTime? Date = null,
    Guid? CompanyId = null,
    Guid? SocietyId = null,
    Guid? ProjectId = null,
    decimal? TaxBase = null,
    decimal? Vat = null,
    string? PaymentTerms = null,
    Guid? StatusId = null,
    string? Responsible = null,
    string? Notes = null) : ICommand<Guid>;
