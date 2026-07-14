using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.CreateMortalidad;

public sealed class CreateMortalidadCommandValidator : AbstractValidator<CreateMortalidadCommand>
{
    public CreateMortalidadCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0);
    }
}
