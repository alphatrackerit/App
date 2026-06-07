using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Incomes;

namespace FSH.Modules.Projects.Features.v1.Incomes.DeleteIncome;

public sealed class DeleteIncomeCommandValidator : AbstractValidator<DeleteIncomeCommand>
{
    public DeleteIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
    }
}
