using FSH.Framework.Shared.Persistence;
using FSH.Modules.Cashflow.Contracts.Enums;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Prefixes;

public sealed record CreatePrefixCommand(
    string Name,
    string? Description = null,
    Guid? PrefixGroupId = null,
    PrefixType Type = PrefixType.Categoria,
    bool IsActive = true) : ICommand<Guid>;

public sealed record UpdatePrefixCommand(
    Guid Id,
    string Name,
    string? Description = null,
    Guid? PrefixGroupId = null,
    PrefixType Type = PrefixType.Categoria,
    bool IsActive = true) : ICommand<Guid>;

public sealed record DeletePrefixCommand(Guid Id) : ICommand<Unit>;

public sealed record PrefixDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? PrefixGroupId,
    PrefixType Type,
    bool IsActive);

public sealed record SearchPrefixesQuery(
    string? Search = null,
    PrefixType? Type = null,
    bool? OnlyActive = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<PrefixDto>>;
