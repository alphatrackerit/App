using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Despachos;

namespace FSH.Modules.Avicola.Features.v1.Despachos.UpdateDespacho;

public sealed class UpdateDespachoCommandValidator : AbstractValidator<UpdateDespachoCommand>
{
    public UpdateDespachoCommandValidator()
    {
        RuleFor(x => x.DespachoId).NotEmpty();
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PesoTotalKg).GreaterThanOrEqualTo(0);
    }
}
