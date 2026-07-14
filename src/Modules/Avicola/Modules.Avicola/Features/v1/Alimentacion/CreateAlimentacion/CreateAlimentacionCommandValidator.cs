using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.CreateAlimentacion;

public sealed class CreateAlimentacionCommandValidator : AbstractValidator<CreateAlimentacionCommand>
{
    public CreateAlimentacionCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.CantidadKg).GreaterThanOrEqualTo(0);
    }
}
