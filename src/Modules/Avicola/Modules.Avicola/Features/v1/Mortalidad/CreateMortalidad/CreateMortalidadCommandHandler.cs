using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.CreateMortalidad;

public sealed class CreateMortalidadCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateMortalidadCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateMortalidadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = RegistroMortalidad.Create(
            command.LoteId,
            command.Fecha,
            command.Cantidad,
            command.Descartes,
            command.Causa,
            command.Notas);

        dbContext.Mortalidades.Add(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
