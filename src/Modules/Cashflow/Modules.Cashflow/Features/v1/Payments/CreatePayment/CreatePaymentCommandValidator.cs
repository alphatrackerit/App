using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Payments;

namespace FSH.Modules.Cashflow.Features.v1.Payments.CreatePayment;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Percentage!.Value).InclusiveBetween(0, 100).When(x => x.Percentage.HasValue);
    }
}
