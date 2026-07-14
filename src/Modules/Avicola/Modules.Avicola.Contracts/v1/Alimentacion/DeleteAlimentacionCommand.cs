using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Alimentacion;

public sealed record DeleteAlimentacionCommand(Guid AlimentacionId) : ICommand<Unit>;
