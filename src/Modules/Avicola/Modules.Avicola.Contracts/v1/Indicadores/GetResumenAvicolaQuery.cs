using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Indicadores;

/// <summary>
/// Cross-flock dashboard summary: one headline row per flock plus tenant-wide rollups. No pagination —
/// the dashboard renders the flock table and the summary cards/charts from this single payload.
/// </summary>
public sealed record GetResumenAvicolaQuery : IQuery<ResumenAvicolaDto>;
