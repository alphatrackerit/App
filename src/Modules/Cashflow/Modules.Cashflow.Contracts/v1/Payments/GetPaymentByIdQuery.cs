using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Payments;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto>;
