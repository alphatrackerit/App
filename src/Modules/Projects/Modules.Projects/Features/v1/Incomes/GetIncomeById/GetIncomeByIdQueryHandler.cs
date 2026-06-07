using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.Dtos;
using FSH.Modules.Projects.Contracts.v1.Incomes;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Incomes.GetIncomeById;

public sealed class GetIncomeByIdQueryHandler(ProjectsDbContext dbContext)
    : IQueryHandler<GetIncomeByIdQuery, IncomeDto>
{
    public async ValueTask<IncomeDto> Handle(GetIncomeByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var i = await dbContext.Incomes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.IncomeId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Income {query.IncomeId} not found.");

        return new IncomeDto(
            i.Id, i.Amount, i.Description, i.Date, i.Percentage, i.ProjectId, i.StatusId, i.Confirmed, i.Validated);
    }
}
