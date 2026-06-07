namespace FSH.Modules.Projects.Contracts.Dtos;

public sealed record IncomeDto(
    Guid Id,
    decimal Amount,
    string? Description,
    DateTimeOffset? Date,
    decimal? Percentage,
    Guid? ProjectId,
    Guid? StatusId,
    bool Confirmed,
    bool Validated);
