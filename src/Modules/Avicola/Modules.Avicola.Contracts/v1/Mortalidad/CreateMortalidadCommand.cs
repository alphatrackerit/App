using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Mortalidad;

public sealed record CreateMortalidadCommand(
    Guid LoteId,
    DateTimeOffset Fecha,
    int Cantidad,
    int? Descartes = null,
    string? Causa = null,
    string? Notas = null) : ICommand<Guid>;
