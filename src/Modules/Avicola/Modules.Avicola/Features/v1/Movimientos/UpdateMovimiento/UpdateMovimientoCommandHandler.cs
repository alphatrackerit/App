using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.UpdateMovimiento;

public sealed class UpdateMovimientoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateMovimientoCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateMovimientoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var movimiento = await dbContext.Movimientos
            .FirstOrDefaultAsync(m => m.Id == command.MovimientoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Movimiento {command.MovimientoId} not found.");

        movimiento.Update(
            command.Fecha,
            command.Tipo,
            command.Categoria,
            command.Concepto,
            command.Importe,
            command.LoteId,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return movimiento.Id;
    }
}
