using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Lotes.UpdateLote;

public sealed class UpdateLoteCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateLoteCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateLoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lote = await dbContext.Lotes
            .FirstOrDefaultAsync(l => l.Id == command.LoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Lote {command.LoteId} not found.");

        lote.Update(
            command.Codigo,
            command.GalponId,
            command.Raza,
            command.FechaIngreso,
            command.CantidadInicial,
            command.PesoInicialGramos,
            command.FechaSalidaPrevista,
            command.Estado,
            command.ProveedorId,
            command.CostoPolluelo,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return lote.Id;
    }
}
