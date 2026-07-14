using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Contabilidad;

/// <summary>
/// Accounting overview: computed costs (chicks, feed, health, received orders) and revenue
/// (dispatches) combined with manual ledger movements, broken down by category and by flock.
/// Optionally scoped to one flock and/or a date range.
/// </summary>
public sealed record GetContabilidadQuery(
    Guid? LoteId = null,
    DateTimeOffset? Desde = null,
    DateTimeOffset? Hasta = null) : IQuery<ContabilidadDto>;
