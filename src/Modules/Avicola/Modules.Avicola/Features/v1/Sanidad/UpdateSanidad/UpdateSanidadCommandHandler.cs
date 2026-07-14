using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.UpdateSanidad;

public sealed class UpdateSanidadCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateSanidadCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateSanidadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.RegistrosSanitarios
            .FirstOrDefaultAsync(s => s.Id == command.SanidadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Sanidad {command.SanidadId} not found.");

        registro.Update(
            command.LoteId,
            command.Fecha,
            command.Tipo,
            command.Producto,
            command.Dosis,
            command.ViaAplicacion,
            command.Costo,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
