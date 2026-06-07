using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Payments;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Payments.DeletePayment;

public sealed class DeletePaymentCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<DeletePaymentCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeletePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Payment {command.PaymentId} not found.");

        dbContext.Payments.Remove(payment);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
