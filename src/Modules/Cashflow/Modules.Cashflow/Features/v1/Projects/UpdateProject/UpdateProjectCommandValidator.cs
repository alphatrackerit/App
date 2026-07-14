using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Projects;

namespace FSH.Modules.Cashflow.Features.v1.Projects.UpdateProject;

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(1024);
    }
}
