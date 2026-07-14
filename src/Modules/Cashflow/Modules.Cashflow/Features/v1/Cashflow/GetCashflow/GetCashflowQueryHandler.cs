using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Cashflow;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Cashflow.GetCashflow;

public sealed class GetCashflowQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetCashflowQuery, CashflowDto>
{
    public async ValueTask<CashflowDto> Handle(GetCashflowQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var projects = await dbContext.Projects.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new CashflowProjectDto(p.Id, p.Name))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var incomes = await dbContext.Incomes.AsNoTracking()
            .Where(i => i.Date != null)
            .OrderBy(i => i.Date)
            .Select(i => new CashflowEntryDto(
                i.Id, i.ProjectId, i.Date!.Value, i.Amount, i.Description, i.Confirmed, i.Validated))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var payments = await dbContext.Payments.AsNoTracking()
            .Where(p => p.Date != null)
            .OrderBy(p => p.Date)
            .Select(p => new CashflowEntryDto(
                p.Id, p.ProjectId, p.Date!.Value, p.Amount, p.Description, p.Confirmed, p.Validated))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new CashflowDto(projects, incomes, payments);
    }
}
