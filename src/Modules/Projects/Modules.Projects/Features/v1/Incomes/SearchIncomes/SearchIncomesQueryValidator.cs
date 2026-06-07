using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Incomes;

namespace FSH.Modules.Projects.Features.v1.Incomes.SearchIncomes;

public sealed class SearchIncomesQueryValidator : AbstractValidator<SearchIncomesQuery>
{
    public SearchIncomesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
