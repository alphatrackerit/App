using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Mortalidad;

public sealed record GetMortalidadByIdQuery(Guid MortalidadId) : IQuery<MortalidadDto>;
