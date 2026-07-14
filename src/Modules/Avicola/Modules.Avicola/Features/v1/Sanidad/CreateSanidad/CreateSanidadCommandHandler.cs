using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.CreateSanidad;

public sealed class CreateSanidadCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateSanidadCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateSanidadCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var registro = RegistroSanitario.Create(
            command.LoteId,
            command.Fecha,
            command.Tipo,
            command.Producto,
            command.Dosis,
            command.ViaAplicacion,
            command.Costo,
            command.Notas);

        dbContext.RegistrosSanitarios.Add(registro);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return registro.Id;
    }
}
