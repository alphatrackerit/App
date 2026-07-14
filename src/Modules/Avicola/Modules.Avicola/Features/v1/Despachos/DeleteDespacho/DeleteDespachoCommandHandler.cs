using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Despachos.DeleteDespacho;

public sealed class DeleteDespachoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteDespachoCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteDespachoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var despacho = await dbContext.Despachos
            .FirstOrDefaultAsync(d => d.Id == command.DespachoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Despacho {command.DespachoId} not found.");

        dbContext.Despachos.Remove(despacho);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
