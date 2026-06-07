using FSH.Framework.Core.Exceptions;
using FSH.Modules.Projects.Contracts.v1.Payments;
using FSH.Modules.Projects.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Projects.Features.v1.Payments.ValidatePayment;

public sealed class ValidatePaymentCommandHandler(ProjectsDbContext dbContext)
    : ICommandHandler<ValidatePaymentCommand, Unit>
{
    public async ValueTask<Unit> Handle(ValidatePaymentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Payment {command.PaymentId} not found.");

        payment.SetValidated(command.Validated);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
