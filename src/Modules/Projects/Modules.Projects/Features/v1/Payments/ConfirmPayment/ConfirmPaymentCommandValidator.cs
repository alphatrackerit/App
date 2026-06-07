using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Payments;

namespace FSH.Modules.Projects.Features.v1.Payments.ConfirmPayment;

public sealed class ConfirmPaymentCommandValidator : AbstractValidator<ConfirmPaymentCommand>
{
    public ConfirmPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
