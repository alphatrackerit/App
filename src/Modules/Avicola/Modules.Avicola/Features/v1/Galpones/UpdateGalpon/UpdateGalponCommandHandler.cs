using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Galpones.UpdateGalpon;

public sealed class UpdateGalponCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateGalponCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateGalponCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var galpon = await dbContext.Galpones
            .FirstOrDefaultAsync(g => g.Id == command.GalponId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Galpon {command.GalponId} not found.");

        galpon.Update(
            command.Nombre,
            command.Codigo,
            command.Capacidad,
            command.SuperficieM2,
            command.Ubicacion,
            command.Activo,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return galpon.Id;
    }
}
