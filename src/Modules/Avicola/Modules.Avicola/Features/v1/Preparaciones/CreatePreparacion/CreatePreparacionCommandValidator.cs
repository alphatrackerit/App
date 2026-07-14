using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.CreatePreparacion;

public sealed class CreatePreparacionCommandValidator : AbstractValidator<CreatePreparacionCommand>
{
    public CreatePreparacionCommandValidator()
    {
        RuleFor(x => x.GalponId).NotEmpty();
    }
}
