using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Lotes;

namespace FSH.Modules.Avicola.Features.v1.Lotes.UpdateLote;

public sealed class UpdateLoteCommandValidator : AbstractValidator<UpdateLoteCommand>
{
    public UpdateLoteCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(128);
        RuleFor(x => x.CantidadInicial).GreaterThanOrEqualTo(0);
    }
}
