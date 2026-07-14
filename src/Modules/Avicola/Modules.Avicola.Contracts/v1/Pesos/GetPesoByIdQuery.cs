using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pesos;

public sealed record GetPesoByIdQuery(Guid PesoId) : IQuery<PesoDto>;
