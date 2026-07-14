using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Projects;

namespace FSH.Modules.Cashflow.Features.v1.Projects.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(1024);
    }
}
