using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pesos.UpdatePeso;

public sealed class UpdatePesoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdatePesoCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdatePesoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.Pesos
            .FirstOrDefaultAsync(p => p.Id == command.PesoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Peso {command.PesoId} not found.");

        registro.Update(
            command.LoteId,
            command.Fecha,
            command.PesoPromedioGramos,
            command.CantidadMuestra,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
