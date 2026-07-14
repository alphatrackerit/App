using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Payments;

public sealed record ValidatePaymentCommand(Guid PaymentId, bool Validated = true) : ICommand<Unit>;
