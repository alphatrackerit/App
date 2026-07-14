using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Preparaciones;

public sealed record DeletePreparacionCommand(Guid PreparacionId) : ICommand<Unit>;
