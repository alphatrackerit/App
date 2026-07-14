using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Indicadores;

/// <summary>Computes the production KPIs (mortality %, FCR, ADG, viability, IEP, costs) for one flock.</summary>
public sealed record GetIndicadoresLoteQuery(Guid LoteId) : IQuery<IndicadoresLoteDto>;
