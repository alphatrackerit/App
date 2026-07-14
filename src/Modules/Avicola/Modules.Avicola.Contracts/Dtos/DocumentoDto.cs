namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record DocumentoDto(
    Guid Id,
    TipoDocumento Tipo,
    DocumentoOrigen Origen,
    Guid? OrigenId,
    Guid? FileAssetId,
    string Url,
    string NombreArchivo,
    string? ContentType,
    DateTimeOffset Fecha,
    string? Notas);
