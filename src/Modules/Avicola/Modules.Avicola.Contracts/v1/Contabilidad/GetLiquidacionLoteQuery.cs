using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Contabilidad;

/// <summary>Full settlement (liquidación) of a flock: production close-out + complete financial result.</summary>
public sealed record GetLiquidacionLoteQuery(Guid LoteId) : IQuery<LiquidacionLoteDto>;
