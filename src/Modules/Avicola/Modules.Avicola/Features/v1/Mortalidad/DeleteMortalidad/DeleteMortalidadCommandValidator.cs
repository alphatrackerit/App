using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.DeleteMortalidad;

public sealed class DeleteMortalidadCommandValidator : AbstractValidator<DeleteMortalidadCommand>
{
    public DeleteMortalidadCommandValidator()
    {
        RuleFor(x => x.MortalidadId).NotEmpty();
    }
}
