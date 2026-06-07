using FSH.Modules.Projects.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Projects;

public sealed record GetProjectByIdQuery(Guid ProjectId) : IQuery<ProjectDto>;
