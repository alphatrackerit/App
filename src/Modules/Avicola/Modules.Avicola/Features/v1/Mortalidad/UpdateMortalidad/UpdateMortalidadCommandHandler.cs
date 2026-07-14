using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.UpdateMortalidad;

public sealed class UpdateMortalidadCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateMortalidadCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateMortalidadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.Mortalidades
            .FirstOrDefaultAsync(m => m.Id == command.MortalidadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Mortalidad {command.MortalidadId} not found.");

        registro.Update(
            command.LoteId,
            command.Fecha,
            command.Cantidad,
            command.Descartes,
            command.Causa,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
