using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Clients;

public sealed record CreateClientCommand(
    string Name,
    string? Code = null,
    string? TaxId = null,
    string? Address = null,
    string? ClientType = null,
    string? Contact = null,
    string? LegalName = null,
    string? Phone = null,
    string? Email = null,
    DateTimeOffset? RegisteredOn = null,
    string? ColorHex = null) : ICommand<Guid>;

public sealed record UpdateClientCommand(
    Guid Id,
    string Name,
    string? Code = null,
    string? TaxId = null,
    string? Address = null,
    string? ClientType = null,
    string? Contact = null,
    string? LegalName = null,
    string? Phone = null,
    string? Email = null,
    DateTimeOffset? RegisteredOn = null,
    string? ColorHex = null) : ICommand<Guid>;

public sealed record DeleteClientCommand(Guid Id) : ICommand<Unit>;

public sealed record ClientDto(
    Guid Id,
    string Name,
    string? Code,
    string? TaxId,
    string? Address,
    string? ClientType,
    string? Contact,
    string? LegalName,
    string? Phone,
    string? Email,
    DateTimeOffset? RegisteredOn,
    string? ColorHex);

public sealed record SearchClientsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<ClientDto>>;
