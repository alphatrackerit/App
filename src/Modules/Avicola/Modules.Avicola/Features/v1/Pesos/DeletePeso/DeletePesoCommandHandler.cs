using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pesos.DeletePeso;

public sealed class DeletePesoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeletePesoCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeletePesoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.Pesos
            .FirstOrDefaultAsync(p => p.Id == command.PesoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Peso {command.PesoId} not found.");

        dbContext.Pesos.Remove(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
