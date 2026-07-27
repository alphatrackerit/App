using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.BankMovements;

public sealed record CreateBankMovementCommand(
    string BankName,
    string Concept,
    decimal Amount,
    decimal Balance,
    DateTime? Date = null) : ICommand<Guid>;

public sealed record UpdateBankMovementCommand(
    Guid Id,
    string BankName,
    string Concept,
    decimal Amount,
    decimal Balance,
    DateTime? Date = null) : ICommand<Guid>;

public sealed record DeleteBankMovementCommand(Guid Id) : ICommand<Unit>;

public sealed record BankMovementDto(
    Guid Id,
    DateTime? Date,
    string Concept,
    decimal Amount,
    decimal Balance,
    string BankName);

public sealed record SearchBankMovementsQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null) : IQuery<PagedResponse<BankMovementDto>>;
