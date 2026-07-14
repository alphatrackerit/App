using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Galpones;

namespace FSH.Modules.Avicola.Features.v1.Galpones.UpdateGalpon;

public sealed class UpdateGalponCommandValidator : AbstractValidator<UpdateGalponCommand>
{
    public UpdateGalponCommandValidator()
    {
        RuleFor(x => x.GalponId).NotEmpty();
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Capacidad).GreaterThanOrEqualTo(0);
    }
}
