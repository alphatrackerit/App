using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Suppliers;

public sealed record CreateSupplierCommand(
    string Name,
    string? Code = null,
    string? TaxId = null,
    string? Address = null,
    string? SupplierType = null,
    string? Contact = null,
    string? LegalName = null,
    string? Phone = null,
    string? Email = null,
    DateTimeOffset? RegisteredOn = null,
    string? ColorHex = null,
    int VisualPriority = 0) : ICommand<Guid>;

public sealed record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? Code = null,
    string? TaxId = null,
    string? Address = null,
    string? SupplierType = null,
    string? Contact = null,
    string? LegalName = null,
    string? Phone = null,
    string? Email = null,
    DateTimeOffset? RegisteredOn = null,
    string? ColorHex = null,
    int VisualPriority = 0) : ICommand<Guid>;

public sealed record DeleteSupplierCommand(Guid Id) : ICommand<Unit>;

public sealed record SupplierDto(
    Guid Id,
    string Name,
    string? Code,
    string? TaxId,
    string? Address,
    string? SupplierType,
    string? Contact,
    string? LegalName,
    string? Phone,
    string? Email,
    DateTimeOffset? RegisteredOn,
    string? ColorHex,
    int VisualPriority);

public sealed record SearchSuppliersQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<SupplierDto>>;
