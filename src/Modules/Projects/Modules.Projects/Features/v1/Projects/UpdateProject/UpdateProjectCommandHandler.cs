using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Projects;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Projects.UpdateProject;

public sealed class UpdateProjectCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<UpdateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateProjectCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var project = await dbContext.Projects
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Project {command.ProjectId} not found.");

        project.Update(
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

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return project.Id;
    }
}
