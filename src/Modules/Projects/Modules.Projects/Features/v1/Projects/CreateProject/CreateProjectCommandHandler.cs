using FSH.Modules.Projects.Contracts.v1.Projects;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Domain;
using Mediator;

namespace FSH.Modules.Projects.Features.v1.Projects.CreateProject;

public sealed class CreateProjectCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<CreateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var project = Project.Create(
            command.Name,
            command.SalePrice,
            command.ForecastSale,
            command.Cost,
            command.ForecastCost,
            command.Profit,
            command.ClientId,
            command.SocietyId,
            command.CountryId,
            command.CompanyId,
            command.StatusId,
            command.PrefixId);

        dbContext.Projects.Add(project);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return project.Id;
    }
}
