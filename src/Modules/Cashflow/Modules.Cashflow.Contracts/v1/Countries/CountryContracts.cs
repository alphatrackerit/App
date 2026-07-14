using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Countries;

public sealed record CreateCountryCommand(string Name, string? Code = null) : ICommand<Guid>;

public sealed record UpdateCountryCommand(Guid Id, string Name, string? Code = null) : ICommand<Guid>;

public sealed record DeleteCountryCommand(Guid Id) : ICommand<Unit>;

public sealed record SearchCountriesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<LookupDto>>;
