using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.UpdatePreparacion;

public sealed class UpdatePreparacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdatePreparacionCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdatePreparacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var preparacion = await dbContext.Preparaciones
            .FirstOrDefaultAsync(p => p.Id == command.PreparacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Preparacion {command.PreparacionId} not found.");

        preparacion.Update(
            command.GalponId,
            command.LoteAnteriorId,
            command.FechaRetiro,
            command.FechaInicio,
            command.RetiradaCama,
            command.Lavado,
            command.Desinfeccion,
            command.Desinsectacion,
            command.CamaNueva,
            command.Costo,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return preparacion.Id;
    }
}
