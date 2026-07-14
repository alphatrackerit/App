using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Alimentacion;

public sealed record GetAlimentacionByIdQuery(Guid AlimentacionId) : IQuery<AlimentacionDto>;
