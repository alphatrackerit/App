using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Contracts.Dtos;

public sealed record InvoiceDto(
    Guid Id,
    string Number,
    string? DynamicsNumber,
    InvoiceType Type,
    DateTime? InvoiceDate,
    DateTime? DueDate,
    Guid? ClientId,
    Guid? SupplierId,
    Guid? CompanyId,
    Guid? SocietyId,
    Guid? ProjectId,
    decimal? TaxBase,
    decimal? Vat,
    decimal Total,
    string? PaymentTerms,
    string? Bank,
    Guid? StatusId,
    bool Verified,
    string? Notes,
    string? DocumentPath,
    decimal Collected);
