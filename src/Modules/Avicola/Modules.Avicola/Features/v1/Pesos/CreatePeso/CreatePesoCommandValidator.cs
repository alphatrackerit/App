using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pesos;

namespace FSH.Modules.Avicola.Features.v1.Pesos.CreatePeso;

public sealed class CreatePesoCommandValidator : AbstractValidator<CreatePesoCommand>
{
    public CreatePesoCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.PesoPromedioGramos).GreaterThanOrEqualTo(0);
    }
}
