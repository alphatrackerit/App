using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Documentos.DeleteDocumento;

public sealed class DeleteDocumentoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<DeleteDocumentoCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteDocumentoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var documento = await dbContext.Documentos
            .FirstOrDefaultAsync(d => d.Id == command.DocumentoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Documento {command.DocumentoId} not found.");

        // The underlying Files FileAsset (if any) is left to the Files module's orphan-purge job.
        dbContext.Documentos.Remove(documento);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
