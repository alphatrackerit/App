using Mediator;

namespace FSH.Modules.Avicola.Contracts.v1.Galpones;

public sealed record DeleteGalponCommand(Guid GalponId) : ICommand<Unit>;
