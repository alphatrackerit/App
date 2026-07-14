using FSH.Modules.Cashflow.Contracts.Dtos;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Projects;

public sealed record GetProjectByIdQuery(Guid ProjectId) : IQuery<ProjectDto>;
