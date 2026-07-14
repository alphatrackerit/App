using FSH.Modules.Avicola.Contracts.v1.Pesos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Pesos.CreatePeso;

public sealed class CreatePesoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreatePesoCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePesoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = RegistroPeso.Create(
            command.LoteId,
            command.Fecha,
            command.PesoPromedioGramos,
            command.CantidadMuestra,
            command.Notas);

        dbContext.Pesos.Add(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
