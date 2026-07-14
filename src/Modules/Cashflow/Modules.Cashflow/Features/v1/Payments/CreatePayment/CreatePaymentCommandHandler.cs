using FSH.Modules.Cashflow.Contracts.v1.Payments;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;

namespace FSH.Modules.Cashflow.Features.v1.Payments.CreatePayment;

public sealed class CreatePaymentCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<CreatePaymentCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = Payment.Create(
            command.Amount,
            command.Description,
            command.Date,
            command.Percentage,
            command.SupplierId,
            command.ProjectId,
            command.StatusId);

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return payment.Id;
    }
}
