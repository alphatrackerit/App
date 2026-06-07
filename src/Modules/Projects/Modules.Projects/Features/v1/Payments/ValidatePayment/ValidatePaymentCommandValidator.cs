using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Payments;

namespace FSH.Modules.Projects.Features.v1.Payments.ValidatePayment;

public sealed class ValidatePaymentCommandValidator : AbstractValidator<ValidatePaymentCommand>
{
    public ValidatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
