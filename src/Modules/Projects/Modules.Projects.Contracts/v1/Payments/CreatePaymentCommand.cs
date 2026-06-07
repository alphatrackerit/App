using Mediator;

namespace FSH.Modules.Projects.Contracts.v1.Payments;

public sealed record CreatePaymentCommand(
    decimal Amount,
    string? Description = null,
    DateTimeOffset? Date = null,
    decimal? Percentage = null,
    Guid? SupplierId = null,
    Guid? ProjectId = null,
    Guid? StatusId = null) : ICommand<Guid>;
