using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Projects;

namespace FSH.Modules.Cashflow.Features.v1.Projects.SearchProjects;

public sealed class SearchProjectsQueryValidator : AbstractValidator<SearchProjectsQuery>
{
    public SearchProjectsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
