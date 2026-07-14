using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Movimientos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Movimientos.DeleteMovimiento;

public sealed class DeleteMovimientoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteMovimientoCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteMovimientoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var movimiento = await dbContext.Movimientos
            .FirstOrDefaultAsync(m => m.Id == command.MovimientoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Movimiento {command.MovimientoId} not found.");

        dbContext.Movimientos.Remove(movimiento);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
