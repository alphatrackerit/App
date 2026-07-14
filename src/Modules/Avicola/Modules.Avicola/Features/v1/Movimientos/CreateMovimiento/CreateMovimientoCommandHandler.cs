using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.CreateMovimiento;

public sealed class CreateMovimientoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateMovimientoCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateMovimientoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var movimiento = MovimientoContable.Create(
            command.Fecha,
            command.Tipo,
            command.Categoria,
            command.Concepto,
            command.Importe,
            command.LoteId,
            command.Notas);

        dbContext.Movimientos.Add(movimiento);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return movimiento.Id;
    }
}
