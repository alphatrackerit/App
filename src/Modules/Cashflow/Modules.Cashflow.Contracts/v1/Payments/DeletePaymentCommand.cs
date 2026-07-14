using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Payments;

public sealed record DeletePaymentCommand(Guid PaymentId) : ICommand<Unit>;
