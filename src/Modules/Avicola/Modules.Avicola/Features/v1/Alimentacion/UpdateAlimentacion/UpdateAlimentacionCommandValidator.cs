using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.UpdateAlimentacion;

public sealed class UpdateAlimentacionCommandValidator : AbstractValidator<UpdateAlimentacionCommand>
{
    public UpdateAlimentacionCommandValidator()
    {
        RuleFor(x => x.AlimentacionId).NotEmpty();
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.CantidadKg).GreaterThanOrEqualTo(0);
    }
}
