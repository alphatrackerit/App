using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Despachos.UpdateDespacho;

public sealed class UpdateDespachoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateDespachoCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateDespachoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var despacho = await dbContext.Despachos
            .FirstOrDefaultAsync(d => d.Id == command.DespachoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Despacho {command.DespachoId} not found.");

        despacho.Update(
            command.LoteId,
            command.Fecha,
            command.Cantidad,
            command.PesoTotalKg,
            command.PrecioPorKg,
            command.ClienteId,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return despacho.Id;
    }
}
