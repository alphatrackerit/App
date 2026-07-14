using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Documentos;

/// <summary>
/// Attaches an uploaded file (already uploaded + finalized via the Files module) as an Avícola
/// document. The client first calls the Files presigned-upload flow, then posts this with the
/// resulting <paramref name="FileAssetId"/> + durable <paramref name="Url"/>.
/// </summary>
public sealed record CreateDocumentoCommand(
    TipoDocumento Tipo,
    DocumentoOrigen Origen,
    Guid? OrigenId,
    Guid? FileAssetId,
    string Url,
    string NombreArchivo,
    string? ContentType = null,
    DateTimeOffset? Fecha = null,
    string? Notas = null) : ICommand<Guid>;
