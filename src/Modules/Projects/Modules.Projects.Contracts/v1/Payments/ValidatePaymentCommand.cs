using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Payments;

public sealed record ValidatePaymentCommand(Guid PaymentId, bool Validated = true) : ICommand<Unit>;
