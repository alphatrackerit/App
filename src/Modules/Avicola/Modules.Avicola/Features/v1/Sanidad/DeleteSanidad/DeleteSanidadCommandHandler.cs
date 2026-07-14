using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.DeleteSanidad;

public sealed class DeleteSanidadCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteSanidadCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteSanidadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.RegistrosSanitarios
            .FirstOrDefaultAsync(s => s.Id == command.SanidadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Sanidad {command.SanidadId} not found.");

        dbContext.RegistrosSanitarios.Remove(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
