using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Projects;

namespace FSH.Modules.Cashflow.Features.v1.Projects.DeleteProject;

public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
    }
}
