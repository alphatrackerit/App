using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.DeleteAlimentacion;

public sealed class DeleteAlimentacionCommandValidator : AbstractValidator<DeleteAlimentacionCommand>
{
    public DeleteAlimentacionCommandValidator()
    {
        RuleFor(x => x.AlimentacionId).NotEmpty();
    }
}
