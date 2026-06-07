using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Incomes;

namespace FSH.Modules.Projects.Features.v1.Incomes.ValidateIncome;

public sealed class ValidateIncomeCommandValidator : AbstractValidator<ValidateIncomeCommand>
{
    public ValidateIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
    }
}
