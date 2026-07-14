using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Lotes.CerrarLote;

public sealed class CerrarLoteCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CerrarLoteCommand, Guid>
{
    public async ValueTask<Guid> Handle(CerrarLoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lote = await dbContext.Lotes
            .FirstOrDefaultAsync(l => l.Id == command.LoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Lote {command.LoteId} not found.");

        lote.Cerrar(command.FechaSalidaReal ?? DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return lote.Id;
    }
}
