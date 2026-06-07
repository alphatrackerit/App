using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Incomes;

public sealed record CreateIncomeCommand(
    decimal Amount,
    string? Description = null,
    DateTimeOffset? Date = null,
    decimal? Percentage = null,
    Guid? ProjectId = null,
    Guid? StatusId = null,
    bool Confirmed = false) : ICommand<Guid>;
