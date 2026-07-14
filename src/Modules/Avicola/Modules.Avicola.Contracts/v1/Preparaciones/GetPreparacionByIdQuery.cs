using FSH.Modules.Avicola.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Preparaciones;

public sealed record GetPreparacionByIdQuery(Guid PreparacionId) : IQuery<PreparacionDto>;
