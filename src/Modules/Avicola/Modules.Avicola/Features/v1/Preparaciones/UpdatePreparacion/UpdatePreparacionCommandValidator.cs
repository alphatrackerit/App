using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.UpdatePreparacion;

public sealed class UpdatePreparacionCommandValidator : AbstractValidator<UpdatePreparacionCommand>
{
    public UpdatePreparacionCommandValidator()
    {
        RuleFor(x => x.PreparacionId).NotEmpty();
        RuleFor(x => x.GalponId).NotEmpty();
    }
}
