using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Incomes;

namespace FSH.Modules.Projects.Features.v1.Incomes.ConfirmIncome;

public sealed class ConfirmIncomeCommandValidator : AbstractValidator<ConfirmIncomeCommand>
{
    public ConfirmIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
    }
}
