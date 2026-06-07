using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Projects;

public sealed record DeleteProjectCommand(Guid ProjectId) : ICommand<Unit>;
