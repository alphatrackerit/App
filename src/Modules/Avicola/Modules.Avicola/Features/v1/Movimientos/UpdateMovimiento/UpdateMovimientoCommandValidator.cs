using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.UpdateMovimiento;

public sealed class UpdateMovimientoCommandValidator : AbstractValidator<UpdateMovimientoCommand>
{
    public UpdateMovimientoCommandValidator()
    {
        RuleFor(x => x.MovimientoId).NotEmpty();
        RuleFor(x => x.Concepto).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Importe).GreaterThanOrEqualTo(0);
    }
}
