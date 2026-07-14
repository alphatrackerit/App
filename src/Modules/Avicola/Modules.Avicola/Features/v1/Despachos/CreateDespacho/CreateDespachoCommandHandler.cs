using FSH.Modules.Avicola.Contracts.v1.Despachos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Despachos.CreateDespacho;

public sealed class CreateDespachoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateDespachoCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateDespachoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var despacho = Despacho.Create(
            command.LoteId,
            command.Fecha,
            command.Cantidad,
            command.PesoTotalKg,
            command.PrecioPorKg,
            command.ClienteId,
            command.Notas);

        dbContext.Despachos.Add(despacho);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return despacho.Id;
    }
}
