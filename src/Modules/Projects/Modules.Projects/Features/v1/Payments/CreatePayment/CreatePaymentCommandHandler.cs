using FSH.Modules.Projects.Contracts.v1.Payments;
using FSH.Modules.Projects.Data;
using FSH.Modules.Projects.Domain;
using Mediator;

namespace FSH.Modules.Projects.Features.v1.Payments.CreatePayment;

public sealed class CreatePaymentCommandHandler(ProjectsDbContext dbContext)
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
