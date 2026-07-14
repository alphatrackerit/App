using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Galpones;

public sealed record GetGalponByIdQuery(Guid GalponId) : IQuery<GalponDto>;
