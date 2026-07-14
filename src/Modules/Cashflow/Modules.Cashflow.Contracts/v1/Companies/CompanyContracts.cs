using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Companies;

public sealed record CreateCompanyCommand(
    string Name,
    string? Code = null,
    string? LegalName = null,
    string? TaxRegistration = null) : ICommand<Guid>;

public sealed record UpdateCompanyCommand(
    Guid Id,
    string Name,
    string? Code = null,
    string? LegalName = null,
    string? TaxRegistration = null) : ICommand<Guid>;

public sealed record DeleteCompanyCommand(Guid Id) : ICommand<Unit>;

public sealed record CompanyDto(Guid Id, string Name, string? Code, string? LegalName, string? TaxRegistration);

public sealed record SearchCompaniesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<CompanyDto>>;
