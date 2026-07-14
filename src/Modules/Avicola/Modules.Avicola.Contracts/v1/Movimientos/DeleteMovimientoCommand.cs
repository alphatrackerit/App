using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Movimientos;

public sealed record DeleteMovimientoCommand(Guid MovimientoId) : ICommand<Unit>;
