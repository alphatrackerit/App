using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Lotes;

namespace FSH.Modules.Avicola.Features.v1.Lotes.CreateLote;

public sealed class CreateLoteCommandValidator : AbstractValidator<CreateLoteCommand>
{
    public CreateLoteCommandValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(128);
        RuleFor(x => x.CantidadInicial).GreaterThanOrEqualTo(0);
    }
}
