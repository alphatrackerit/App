using FSH.Modules.Avicola.Contracts.v1.Galpones;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Galpones.CreateGalpon;

public sealed class CreateGalponCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateGalponCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateGalponCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var galpon = Galpon.Create(
            command.Nombre,
            command.Codigo,
            command.Capacidad,
            command.SuperficieM2,
            command.Ubicacion,
            command.Activo,
            command.Notas);

        dbContext.Galpones.Add(galpon);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return galpon.Id;
    }
}
