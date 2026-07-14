using FSH.Modules.Avicola.Contracts.v1.Documentos;
using FSH.Modules.Avicola.Data;
using FSH.Modules.Avicola.Domain;
using Mediator;

namespace FSH.Modules.Avicola.Features.v1.Documentos.CreateDocumento;

public sealed class CreateDocumentoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<CreateDocumentoCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateDocumentoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var documento = Documento.Create(
            command.Tipo,
            command.Origen,
            command.OrigenId,
            command.FileAssetId,
            command.Url,
            command.NombreArchivo,
            command.ContentType,
            command.Fecha ?? DateTimeOffset.UtcNow,
            command.Notas);

        dbContext.Documentos.Add(documento);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return documento.Id;
    }
}
