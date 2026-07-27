using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Banks;

public sealed record CreateBankCommand(
    string Name,
    int StartRow = 1,
    int DateColumn = 0,
    int ConceptColumn = 1,
    int AmountColumn = 2,
    int BalanceColumn = 3,
    bool IsActive = true,
    string? Notes = null) : ICommand<Guid>;

public sealed record UpdateBankCommand(
    Guid Id,
    string Name,
    int StartRow = 1,
    int DateColumn = 0,
    int ConceptColumn = 1,
    int AmountColumn = 2,
    int BalanceColumn = 3,
    bool IsActive = true,
    string? Notes = null) : ICommand<Guid>;

public sealed record DeleteBankCommand(Guid Id) : ICommand<Unit>;

public sealed record BankDto(
    Guid Id,
    string Name,
    int StartRow,
    int DateColumn,
    int ConceptColumn,
    int AmountColumn,
    int BalanceColumn,
    bool IsActive,
    string? Notes);

public sealed record SearchBanksQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<BankDto>>;
