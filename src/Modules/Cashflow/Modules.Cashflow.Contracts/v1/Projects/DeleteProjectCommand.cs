using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Projects;

public sealed record DeleteProjectCommand(Guid ProjectId) : ICommand<Unit>;
