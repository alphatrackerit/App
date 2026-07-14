using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Lotes;

public sealed record GetLoteByIdQuery(Guid LoteId) : IQuery<LoteDto>;
