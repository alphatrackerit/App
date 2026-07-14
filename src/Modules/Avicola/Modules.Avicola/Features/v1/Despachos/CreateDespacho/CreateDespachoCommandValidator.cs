using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Despachos;

namespace FSH.Modules.Avicola.Features.v1.Despachos.CreateDespacho;

public sealed class CreateDespachoCommandValidator : AbstractValidator<CreateDespachoCommand>
{
    public CreateDespachoCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Cantidad).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PesoTotalKg).GreaterThanOrEqualTo(0);
    }
}
