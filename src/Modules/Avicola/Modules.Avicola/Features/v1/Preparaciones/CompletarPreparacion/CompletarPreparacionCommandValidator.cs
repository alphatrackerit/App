using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.CompletarPreparacion;

public sealed class CompletarPreparacionCommandValidator : AbstractValidator<CompletarPreparacionCommand>
{
    public CompletarPreparacionCommandValidator()
    {
        RuleFor(x => x.PreparacionId).NotEmpty();
    }
}
