using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Payments.CreatePayment;

public sealed class CreatePaymentCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreatePaymentCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Default to the PENDIENTE (Pago) status when none is supplied.
        var statusId = command.StatusId ?? await dbContext.Statuses.AsNoTracking()
            .Where(s => s.Type == StatusType.Pago && s.Name == "PENDIENTE")
            .Select(s => (Guid?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var payment = Payment.Create(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.SupplierId,
            command.ProjectId,
            statusId,
            command.Confirmed);

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return payment.Id;
    }
}
