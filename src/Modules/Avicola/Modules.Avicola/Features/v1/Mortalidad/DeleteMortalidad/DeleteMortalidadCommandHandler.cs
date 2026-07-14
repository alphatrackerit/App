using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.DeleteMortalidad;

public sealed class DeleteMortalidadCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteMortalidadCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteMortalidadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.Mortalidades
            .FirstOrDefaultAsync(m => m.Id == command.MortalidadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Mortalidad {command.MortalidadId} not found.");

        dbContext.Mortalidades.Remove(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
