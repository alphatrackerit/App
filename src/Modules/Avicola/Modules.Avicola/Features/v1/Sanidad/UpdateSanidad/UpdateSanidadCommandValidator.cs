using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.UpdateSanidad;

public sealed class UpdateSanidadCommandValidator : AbstractValidator<UpdateSanidadCommand>
{
    public UpdateSanidadCommandValidator()
    {
        RuleFor(x => x.SanidadId).NotEmpty();
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Producto).NotEmpty().MaximumLength(256);
    }
}
