using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Mortalidad;

public sealed record DeleteMortalidadCommand(Guid MortalidadId) : ICommand<Unit>;
