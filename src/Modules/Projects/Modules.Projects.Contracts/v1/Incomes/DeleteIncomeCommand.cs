using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Incomes;

public sealed record DeleteIncomeCommand(Guid IncomeId) : ICommand<Unit>;
