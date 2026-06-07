namespace FSH.Modules.Projects.Contracts.Dtos;

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    string? Description,
    DateTimeOffset? Date,
    decimal? Percentage,
    Guid? SupplierId,
    Guid? ProjectId,
    Guid? StatusId,
    bool Confirmed,
    bool Validated);
