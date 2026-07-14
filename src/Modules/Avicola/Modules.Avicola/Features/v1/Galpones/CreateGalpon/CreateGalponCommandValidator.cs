using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Galpones;

namespace FSH.Modules.Avicola.Features.v1.Galpones.CreateGalpon;

public sealed class CreateGalponCommandValidator : AbstractValidator<CreateGalponCommand>
{
    public CreateGalponCommandValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Capacidad).GreaterThanOrEqualTo(0);
    }
}
