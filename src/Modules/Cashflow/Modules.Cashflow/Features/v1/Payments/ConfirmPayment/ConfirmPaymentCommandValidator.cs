using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Payments;

namespace FSH.Modules.Cashflow.Features.v1.Payments.ConfirmPayment;

public sealed class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
