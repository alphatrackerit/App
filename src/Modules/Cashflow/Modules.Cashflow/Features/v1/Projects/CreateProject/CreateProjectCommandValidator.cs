using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Projects;

namespace FSH.Modules.Cashflow.Features.v1.Projects.CreateProject;

public sealed class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(1024);

        // Spec §3.5 / §5.1: a project needs a Cliente and a País. Estado defaults to PREVISTO
        // in the handler when omitted; Categoría (PrefixId) becomes required once the dashboard
        // form exposes its dropdown (Fase 5).
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.CountryId).NotEmpty();
    }
}
