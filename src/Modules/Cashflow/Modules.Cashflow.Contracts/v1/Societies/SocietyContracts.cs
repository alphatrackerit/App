using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Societies;

public sealed record CreateSocietyCommand(
    string Name,
    string? TaxId = null,
    string? Address = null,
    string? PostalCode = null,
    string? City = null,
    string? Country = null,
    Guid? ClientId = null) : ICommand<Guid>;

public sealed record UpdateSocietyCommand(
    Guid Id,
    string Name,
    string? TaxId = null,
    string? Address = null,
    string? PostalCode = null,
    string? City = null,
    string? Country = null,
    Guid? ClientId = null) : ICommand<Guid>;

public sealed record DeleteSocietyCommand(Guid Id) : ICommand<Unit>;

public sealed record SocietyDto(
    Guid Id,
    string Name,
    string? TaxId,
    string? Address,
    string? PostalCode,
    string? City,
    string? Country,
    Guid? ClientId);

public sealed record SearchSocietiesQuery(
    string? Search = null,
    Guid? ClientId = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<SocietyDto>>;
