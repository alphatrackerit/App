using FSH.Framework.Core.Domain;

namespace FSH.Modules.Projects.Domain;

public sealed class Income : AggregateRoot<Guid>
{
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset? Date { get; private set; }
    public decimal? Percentage { get; private set; }

    /// <summary>Owning project (same module → real FK, ON DELETE NO ACTION). Nullable per spec.</summary>
    public Guid? ProjectId { get; private set; }

    /// <summary>Soft reference to the external Estados catalog (indexed, no FK).</summary>
    public Guid? StatusId { get; private set; }

    public bool Confirmed { get; private set; }
    public bool Validated { get; private set; }

    private Income() { }

    public static Income Create(
        decimal amount,
        string? description,
        DateTimeOffset? date,
        decimal? percentage,
        Guid? projectId,
        Guid? statusId,
        bool confirmed)
    {
        return new Income
        {
            Id = Guid.CreateVersion7(),
            Amount = amount,
            Description = description?.Trim(),
            Date = date,
            Percentage = percentage,
            ProjectId = projectId,
            StatusId = statusId,
            Confirmed = confirmed,
            Validated = false,
        };
    }

    public void Update(
        decimal amount,
        string? description,
        DateTimeOffset? date,
        decimal? percentage,
        Guid? projectId,
        Guid? statusId,
        bool confirmed)
    {
        Amount = amount;
        Description = description?.Trim();
        Date = date;
        Percentage = percentage;
        ProjectId = projectId;
        StatusId = statusId;
        Confirmed = confirmed;
    }

    public void SetConfirmed(bool value) => Confirmed = value;

    public void SetValidated(bool value) => Validated = value;
}
