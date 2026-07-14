using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.DeleteAlimentacion;

public sealed class DeleteAlimentacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteAlimentacionCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteAlimentacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.Alimentaciones
            .FirstOrDefaultAsync(a => a.Id == command.AlimentacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Alimentacion {command.AlimentacionId} not found.");

        dbContext.Alimentaciones.Remove(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
