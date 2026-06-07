using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Incomes;

public sealed record ConfirmIncomeCommand(Guid IncomeId, bool Confirmed = true) : ICommand<Unit>;
