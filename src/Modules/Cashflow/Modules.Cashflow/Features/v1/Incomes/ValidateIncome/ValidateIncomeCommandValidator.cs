using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.ValidateIncome;

public sealed class ValidateIncomeCommandValidator : AbstractValidator<ValidateIncomeCommand>
{
    public ValidateIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
    }
}
