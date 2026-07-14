using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Lotes.DeleteLote;

public sealed class DeleteLoteCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteLoteCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteLoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lote = await dbContext.Lotes
            .FirstOrDefaultAsync(l => l.Id == command.LoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Lote {command.LoteId} not found.");

        dbContext.Lotes.Remove(lote);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
