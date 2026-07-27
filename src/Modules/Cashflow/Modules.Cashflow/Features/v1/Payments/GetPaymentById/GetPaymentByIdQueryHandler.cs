using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Payments.GetPaymentById;

public sealed class GetPaymentByIdQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetPaymentByIdQuery, PaymentDto>
{
    public async ValueTask<PaymentDto> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var p = await dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.PaymentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Payment {query.PaymentId} not found.");

        return new PaymentDto(
            p.Id, p.Amount, p.Description, p.Date, p.Percentage, p.SupplierId, p.ProjectId, p.StatusId, p.Confirmed, p.Validated, p.InvoiceId);
    }
}
