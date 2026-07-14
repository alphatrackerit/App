using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.CreateMovimiento;

public sealed class CreateMovimientoCommandValidator : AbstractValidator<CreateMovimientoCommand>
{
    public CreateMovimientoCommandValidator()
    {
        RuleFor(x => x.Concepto).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Importe).GreaterThanOrEqualTo(0);
    }
}
