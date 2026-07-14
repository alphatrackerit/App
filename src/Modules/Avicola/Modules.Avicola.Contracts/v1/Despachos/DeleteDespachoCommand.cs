using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Despachos;

public sealed record DeleteDespachoCommand(Guid DespachoId) : ICommand<Unit>;
