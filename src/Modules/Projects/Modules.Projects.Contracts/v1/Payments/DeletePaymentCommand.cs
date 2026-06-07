using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Payments;

public sealed record DeletePaymentCommand(Guid PaymentId) : ICommand<Unit>;
