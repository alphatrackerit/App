using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Sanidad;

public sealed record DeleteSanidadCommand(Guid SanidadId) : ICommand<Unit>;
