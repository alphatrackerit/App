using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Projects;

namespace FSH.Modules.Projects.Features.v1.Projects.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(1024);
    }
}
