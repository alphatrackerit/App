using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.DeletePreparacion;

public sealed class DeletePreparacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeletePreparacionCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeletePreparacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var preparacion = await dbContext.Preparaciones
            .FirstOrDefaultAsync(p => p.Id == command.PreparacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Preparacion {command.PreparacionId} not found.");

        dbContext.Preparaciones.Remove(preparacion);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
