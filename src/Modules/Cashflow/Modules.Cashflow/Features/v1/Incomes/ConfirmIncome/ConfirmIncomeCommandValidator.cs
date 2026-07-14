using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.ConfirmIncome;

public sealed class ConfirmIncomeCommandValidator : AbstractValidator<ConfirmIncomeCommand>
{
    public ConfirmIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
    }
}
