using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Payments;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Payments.UpdatePayment;

public sealed class UpdatePaymentCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<UpdatePaymentCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdatePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Payment {command.PaymentId} not found.");

        payment.Update(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.SupplierId,
            command.ProjectId,
            command.StatusId);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return payment.Id;
    }
}
