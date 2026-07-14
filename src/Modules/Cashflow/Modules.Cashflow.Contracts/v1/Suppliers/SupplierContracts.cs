using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Suppliers;

public sealed record CreateSupplierCommand(string Name, string? Code = null) : ICommand<Guid>;

public sealed record UpdateSupplierCommand(Guid Id, string Name, string? Code = null) : ICommand<Guid>;

public sealed record DeleteSupplierCommand(Guid Id) : ICommand<Unit>;

public sealed record SearchSuppliersQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<LookupDto>>;
