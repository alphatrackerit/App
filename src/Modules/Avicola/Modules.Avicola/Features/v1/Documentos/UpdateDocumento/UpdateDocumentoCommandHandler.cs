using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.v1.Documentos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Documentos.UpdateDocumento;

public sealed class UpdateDocumentoCommandHandler(AvicolaDbContext dbContext)
    : ICommandHandler<UpdateDocumentoCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateDocumentoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var documento = await dbContext.Documentos
            .FirstOrDefaultAsync(d => d.Id == command.DocumentoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Documento {command.DocumentoId} not found.");

        documento.Update(command.Tipo, command.Notas);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return documento.Id;
    }
}
