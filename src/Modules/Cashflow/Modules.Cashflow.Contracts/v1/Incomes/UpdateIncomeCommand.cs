using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Incomes;

public sealed record UpdateIncomeCommand(
    Guid IncomeId,
    decimal Amount,
    string? Description = null,
    DateTimeOffset? Date = null,
    decimal? Percentage = null,
    Guid? ProjectId = null,
    Guid? StatusId = null,
    bool Confirmed = false) : ICommand<Guid>;
