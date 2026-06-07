using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Payments;

public sealed record ConfirmPaymentCommand(Guid PaymentId, bool Confirmed = true) : ICommand<Unit>;
