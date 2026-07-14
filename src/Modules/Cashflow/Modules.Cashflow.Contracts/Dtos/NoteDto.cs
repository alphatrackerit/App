namespace FSH.Modules.Cashflow.Contracts.Dtos;

public sealed record NoteDto(
    Guid Id,
    Guid? ProjectId,
    string Title,
    string? Description,
    DateTimeOffset Date);
