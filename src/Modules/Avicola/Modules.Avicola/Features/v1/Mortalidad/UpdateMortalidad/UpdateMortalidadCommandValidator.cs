using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.UpdateMortalidad;

public sealed class UpdateMortalidadCommandValidator : AbstractValidator<UpdateMortalidadCommand>
{
    public UpdateMortalidadCommandValidator()
    {
        RuleFor(x => x.MortalidadId).NotEmpty();
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0);
    }
}
