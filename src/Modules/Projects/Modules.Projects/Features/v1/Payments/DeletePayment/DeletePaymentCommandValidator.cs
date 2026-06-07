using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Payments;

namespace FSH.Modules.Projects.Features.v1.Payments.DeletePayment;

public sealed class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId).NotEmpty();
    }
}
