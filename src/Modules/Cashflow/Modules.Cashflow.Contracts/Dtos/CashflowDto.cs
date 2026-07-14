namespace FSH.Modules.Cashflow.Contracts.Dtos;

/// <summary>
/// Raw cash-flow ledger for the dashboard grid: the project list (for the selector) plus every
/// dated income and payment. The client groups by day, filters by project/status and computes the
/// running accumulated balance, so the grid can switch views without re-querying.
/// </summary>
public sealed record CashflowDto(
    IReadOnlyList<CashflowProjectDto> Projects,
    IReadOnlyList<CashflowEntryDto> Incomes,
    IReadOnlyList<CashflowEntryDto> Payments);

public sealed record CashflowProjectDto(Guid Id, string Name);

public sealed record CashflowEntryDto(
    Guid Id,
    Guid? ProjectId,
    DateTime Date,
    decimal Amount,
    string? Description,
    bool Confirmed,
    bool Validated);
