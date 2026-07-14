using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Statuses;

public sealed record CreateStatusCommand(string Name, string? Code = null) : ICommand<Guid>;

public sealed record UpdateStatusCommand(Guid Id, string Name, string? Code = null) : ICommand<Guid>;

public sealed record DeleteStatusCommand(Guid Id) : ICommand<Unit>;

public sealed record SearchStatusesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<LookupDto>>;
