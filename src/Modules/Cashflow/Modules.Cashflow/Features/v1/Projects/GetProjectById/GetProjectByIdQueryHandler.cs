using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Projects;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Projects.GetProjectById;

public sealed class GetProjectByIdQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    public async ValueTask<ProjectDto> Handle(GetProjectByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var p = await dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.ProjectId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Project {query.ProjectId} not found.");

        return new ProjectDto(
            p.Id, p.Name, p.SalePrice, p.ForecastSale, p.Cost, p.ForecastCost, p.Profit,
            p.ClientId, p.SocietyId, p.CountryId, p.CompanyId, p.StatusId, p.PrefixId);
    }
}
