using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Payments;

namespace FSH.Modules.Cashflow.Features.v1.Payments.DeletePayment;

public sealed class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
