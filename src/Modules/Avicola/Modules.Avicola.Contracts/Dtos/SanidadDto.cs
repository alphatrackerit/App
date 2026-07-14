namespace FSH.Modules.Avicola.Contracts.Dtos;

public sealed record SanidadDto(
    Guid Id,
    Guid LoteId,
    DateTimeOffset Fecha,
    TipoRegistroSanitario Tipo,
    string Producto,
    string? Dosis,
    string? ViaAplicacion,
    decimal? Costo,
    string? Notas);
