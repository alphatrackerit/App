using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Galpones.DeleteGalpon;

public sealed class DeleteGalponCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteGalponCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteGalponCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var galpon = await dbContext.Galpones
            .FirstOrDefaultAsync(g => g.Id == command.GalponId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Galpon {command.GalponId} not found.");

        dbContext.Galpones.Remove(galpon);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
