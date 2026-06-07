using FluentValidation;
using FSH.Modules.Projects.Contracts.v1.Projects;

namespace FSH.Modules.Projects.Features.v1.Projects.DeleteProject;

public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
