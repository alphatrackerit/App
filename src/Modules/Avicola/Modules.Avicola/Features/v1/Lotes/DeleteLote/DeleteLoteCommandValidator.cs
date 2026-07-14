using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Lotes;

namespace FSH.Modules.Avicola.Features.v1.Lotes.DeleteLote;

public sealed class DeleteLoteCommandValidator : AbstractValidator<DeleteLoteCommand>
{
    public DeleteLoteCommandValidator()
    {
        RuleFor(x => x.LoteId).NotEmpty();
    }
}
