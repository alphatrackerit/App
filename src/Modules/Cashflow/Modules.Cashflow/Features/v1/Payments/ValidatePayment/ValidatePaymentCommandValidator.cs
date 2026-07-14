using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Payments;

namespace FSH.Modules.Cashflow.Features.v1.Payments.ValidatePayment;

public sealed class ValidatePaymentCommandValidator : AbstractValidator<ValidatePaymentCommand>
{
    public ValidatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
