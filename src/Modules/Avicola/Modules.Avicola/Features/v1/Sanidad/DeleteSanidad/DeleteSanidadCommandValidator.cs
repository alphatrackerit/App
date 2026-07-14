using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.DeleteSanidad;

public sealed class DeleteSanidadCommandValidator : AbstractValidator<DeleteSanidadCommand>
{
    public DeleteSanidadCommandValidator()
    {
        RuleFor(x => x.SanidadId).NotEmpty();
    }
}
