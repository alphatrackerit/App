using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.DeleteIncome;

public sealed class DeleteIncomeCommandValidator : AbstractValidator<DeleteIncomeCommand>
{
    public DeleteIncomeCommandValidator()
    {
        RuleFor(x => x.IncomeId).NotEmpty();
    }
}
