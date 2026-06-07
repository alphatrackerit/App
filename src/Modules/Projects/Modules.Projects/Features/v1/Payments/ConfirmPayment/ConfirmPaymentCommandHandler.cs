using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Payments;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Payments.ConfirmPayment;

public sealed class ConfirmPaymentCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<ConfirmPaymentCommand, Unit>
{
    public async ValueTask<Unit> Handle(ConfirmPaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Payment {command.PaymentId} not found.");

        payment.SetConfirmed(command.Confirmed);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
