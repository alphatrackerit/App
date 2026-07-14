using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.DeleteMovimiento;

public sealed class DeleteMovimientoCommandValidator : AbstractValidator<DeleteMovimientoCommand>
{
    public DeleteMovimientoCommandValidator()
    {
        RuleFor(x => x.MovimientoId).NotEmpty();
    }
}
