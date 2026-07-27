using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Incomes;

public sealed record CreateIncomeCommand(
    decimal Amount,
    string? Description = null,
    DateTime? Date = null,
    decimal? Percentage = null,
    Guid? ProjectId = null,
    Guid? StatusId = null,
    Guid? InvoiceId = null,
    bool Confirmed = false) : ICommand<Guid>;
