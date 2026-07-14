using FSH.Modules.Avicola.Contracts.v1.Lotes;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Lotes.CreateLote;

public sealed class CreateLoteCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateLoteCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateLoteCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var lote = Lote.Create(
            command.Codigo,
            command.GalponId,
            command.Raza,
            command.FechaIngreso,
            command.CantidadInicial,
            command.PesoInicialGramos,
            command.FechaSalidaPrevista,
            command.Estado,
            command.ProveedorId,
            command.CostoPolluelo,
            command.Notas);

        dbContext.Lotes.Add(lote);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return lote.Id;
    }
}
