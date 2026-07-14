using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pesos;

namespace FSH.Modules.Avicola.Features.v1.Pesos.DeletePeso;

public sealed class DeletePesoCommandValidator : AbstractValidator<DeletePesoCommand>
{
    public DeletePesoCommandValidator()
    {
        RuleFor(x => x.PesoId).NotEmpty();
    }
}
