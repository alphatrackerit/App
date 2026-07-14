namespace FSH.Modules.Cashflow.Contracts.Dtos;

public sealed record PaymentDto(
    Guid Id,
    decimal Amount,
    string? Description,
    DateTime? Date,
    decimal? Percentage,
    Guid? SupplierId,
    Guid? ProjectId,
    Guid? StatusId,
    bool Confirmed,
    bool Validated);
