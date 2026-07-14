using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Despachos;

public sealed record GetDespachoByIdQuery(Guid DespachoId) : IQuery<DespachoDto>;
