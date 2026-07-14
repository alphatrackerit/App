using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.CreateSanidad;

public sealed class CreateSanidadCommandValidator : AbstractValidator<CreateSanidadCommand>
{
    public CreateSanidadCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Producto).NotEmpty().MaximumLength(256);
    }
}
