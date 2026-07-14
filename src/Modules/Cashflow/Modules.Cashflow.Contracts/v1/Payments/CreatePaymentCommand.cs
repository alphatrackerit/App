using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Payments;

public sealed record CreatePaymentCommand(
    decimal Amount,
    string? Description = null,
    DateTime? Date = null,
    decimal? Percentage = null,
    Guid? SupplierId = null,
    Guid? ProjectId = null,
    Guid? StatusId = null,
    bool Confirmed = false) : ICommand<Guid>;
