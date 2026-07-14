using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.CompletarPreparacion;

public sealed class CompletarPreparacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CompletarPreparacionCommand, Guid>
{
    public async ValueTask<Guid> Handle(CompletarPreparacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var preparacion = await dbContext.Preparaciones
            .FirstOrDefaultAsync(p => p.Id == command.PreparacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Preparacion {command.PreparacionId} not found.");

        preparacion.Completar(command.FechaFin ?? DateTimeOffset.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return preparacion.Id;
    }
}
