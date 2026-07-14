using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.CreateAlimentacion;

public sealed class CreateAlimentacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateAlimentacionCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateAlimentacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = RegistroAlimentacion.Create(
            command.LoteId,
            command.Fecha,
            command.TipoAlimento,
            command.CantidadKg,
            command.CostoUnitario,
            command.Notas);

        dbContext.Alimentaciones.Add(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
