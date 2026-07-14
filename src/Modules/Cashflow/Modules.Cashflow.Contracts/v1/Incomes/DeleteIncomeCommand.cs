using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Incomes;

public sealed record DeleteIncomeCommand(Guid IncomeId) : ICommand<Unit>;
