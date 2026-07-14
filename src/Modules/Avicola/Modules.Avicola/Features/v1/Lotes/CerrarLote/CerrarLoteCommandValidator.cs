using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Lotes;

namespace FSH.Modules.Avicola.Features.v1.Lotes.CerrarLote;

public sealed class CerrarLoteCommandValidator : AbstractValidator<CerrarLoteCommand>
{
    public CerrarLoteCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
    }
}
