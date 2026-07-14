using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Pedidos;

public sealed record DeletePedidoCommand(Guid PedidoId) : ICommand<Unit>;
