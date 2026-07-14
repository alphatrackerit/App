using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pesos;

public sealed record DeletePesoCommand(Guid PesoId) : ICommand<Unit>;
