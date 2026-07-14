using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.UpdateAlimentacion;

public sealed class UpdateAlimentacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateAlimentacionCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateAlimentacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = await dbContext.Alimentaciones
            .FirstOrDefaultAsync(a => a.Id == command.AlimentacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Alimentacion {command.AlimentacionId} not found.");

        registro.Update(
            command.LoteId,
            command.Fecha,
            command.TipoAlimento,
            command.CantidadKg,
            command.CostoUnitario,
            command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
