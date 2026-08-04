using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Proformas;

/// <summary>Updates a proforma's editable header fields. The proforma <c>Type</c> is immutable.</summary>
public sealed record UpdateProformaCommand(
    Guid ProformaId,
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
