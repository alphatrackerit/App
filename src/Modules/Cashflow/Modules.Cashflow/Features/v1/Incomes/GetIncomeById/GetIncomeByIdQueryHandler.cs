using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Incomes;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Incomes.GetIncomeById;

public sealed class GetIncomeByIdQueryHandler(CashflowDbContext dbContext)
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
            i.Id, i.Amount, i.Description, i.Date, i.Percentage, i.ProjectId, i.StatusId, i.Confirmed, i.Validated, i.InvoiceId);
    }
}
