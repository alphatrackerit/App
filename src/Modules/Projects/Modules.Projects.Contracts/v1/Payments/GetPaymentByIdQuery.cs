using FSH.Modules.Projects.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Payments;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto>;
