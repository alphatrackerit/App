using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.CreatePreparacion;

public sealed class CreatePreparacionCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreatePreparacionCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePreparacionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var preparacion = PreparacionNave.Create(
            command.GalponId,
            command.LoteAnteriorId,
            command.FechaRetiro,
            command.FechaInicio == default ? DateTimeOffset.UtcNow : command.FechaInicio,
            command.RetiradaCama,
            command.Lavado,
            command.Desinfeccion,
            command.Desinsectacion,
            command.CamaNueva,
            command.Costo,
            command.Estado,
            command.Notas);

        dbContext.Preparaciones.Add(preparacion);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return preparacion.Id;
    }
}
