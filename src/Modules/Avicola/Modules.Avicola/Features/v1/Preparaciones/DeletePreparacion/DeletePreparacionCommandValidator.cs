using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.DeletePreparacion;

public sealed class DeletePreparacionCommandValidator : AbstractValidator<DeletePreparacionCommand>
{
    public DeletePreparacionCommandValidator()
    {
        RuleFor(x => x.PreparacionId).NotEmpty();
    }
}
