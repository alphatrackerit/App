using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.UpdateIncome;

public sealed class UpdateIncomeCommandValidator : AbstractValidator<UpdateIncomeCommand>
{
    public UpdateIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Percentage!.Value).InclusiveBetween(0, 100).When(x => x.Percentage.HasValue);
    }
}
