using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Payments;

public sealed record UpdatePaymentCommand(
    Guid PaymentId,
    decimal Amount,
    string? Description = null,
    DateTime? Date = null,
    decimal? Percentage = null,
    Guid? SupplierId = null,
    Guid? ProjectId = null,
    Guid? StatusId = null) : ICommand<Guid>;
