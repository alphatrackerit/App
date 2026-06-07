using FSH.Framework.Shared.Persistence;
using FSH.Modules.Administration.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Administration.Contracts.v1.Clients;

public sealed record CreateClientCommand(string Name, string? Code = null) : ICommand<Guid>;

public sealed record UpdateClientCommand(Guid Id, string Name, string? Code = null) : ICommand<Guid>;

public sealed record DeleteClientCommand(Guid Id) : ICommand<Unit>;

public sealed record SearchClientsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<LookupDto>>;
