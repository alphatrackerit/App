using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Lotes;

public sealed record DeleteLoteCommand(Guid LoteId) : ICommand<Unit>;
