using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Sanidad;

public sealed record UpdateSanidadCommand(
    Guid SanidadId,
    Guid LoteId,
    DateTimeOffset Fecha,
    TipoRegistroSanitario Tipo,
    string Producto,
    string? Dosis = null,
    string? ViaAplicacion = null,
    decimal? Costo = null,
    string? Notas = null) : ICommand<Guid>;
