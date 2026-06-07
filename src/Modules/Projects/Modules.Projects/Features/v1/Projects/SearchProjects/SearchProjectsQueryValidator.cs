using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Projects;

namespace FSH.Modules.Projects.Features.v1.Projects.SearchProjects;

public sealed class SearchProjectsQueryValidator : AbstractValidator<SearchProjectsQuery>
{
    public SearchProjectsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
