using FSH.Framework.Shared.Persistence;
using FSH.Modules.Administration.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Administration.Contracts.v1.Companies;

public sealed record CreateCompanyCommand(string Name, string? Code = null) : ICommand<Guid>;

public sealed record UpdateCompanyCommand(Guid Id, string Name, string? Code = null) : ICommand<Guid>;

public sealed record DeleteCompanyCommand(Guid Id) : ICommand<Unit>;

public sealed record SearchCompaniesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<LookupDto>>;
