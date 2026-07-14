using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pesos;

namespace FSH.Modules.Avicola.Features.v1.Pesos.UpdatePeso;

public sealed class UpdatePesoCommandValidator : AbstractValidator<UpdatePesoCommand>
{
    public UpdatePesoCommandValidator()
    {
        RuleFor(x => x.PesoId).NotEmpty();
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.PesoPromedioGramos).GreaterThanOrEqualTo(0);
    }
}
