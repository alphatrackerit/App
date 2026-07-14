using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Documentos;

public sealed record DeleteDocumentoCommand(Guid DocumentoId) : ICommand<Unit>;
