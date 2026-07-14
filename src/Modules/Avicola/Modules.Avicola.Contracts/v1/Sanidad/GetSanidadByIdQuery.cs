using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Sanidad;

public sealed record GetSanidadByIdQuery(Guid SanidadId) : IQuery<SanidadDto>;
