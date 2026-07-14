using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Documentos;

public sealed record UpdateDocumentoCommand(
    Guid DocumentoId,
    TipoDocumento Tipo,
    string? Notas = null) : ICommand<Guid>;
