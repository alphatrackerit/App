using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Incomes;

public sealed record ValidateIncomeCommand(Guid IncomeId, bool Validated = true) : ICommand<Unit>;
