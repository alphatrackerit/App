using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Note : AggregateRoot<Guid>
{
    /// <summary>Owning project (same module → real FK, ON DELETE NO ACTION). Nullable per spec.</summary>
    public Guid? ProjectId { get; private set; }

    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }

    /// <summary>Required. Defaults to <see cref="DateTimeOffset.MinValue"/> (stored as Postgres -infinity).</summary>
    public DateTimeOffset Date { get; private set; }

    private Note() { }

    public static Note Create(Guid? projectId, string title, string? description, DateTimeOffset? date)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new Note
        {
            Id = Guid.CreateVersion7(),
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Date = date ?? DateTimeOffset.MinValue,
        };
    }

    public void Update(Guid? projectId, string title, string? description, DateTimeOffset? date)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        ProjectId = projectId;
        Title = title.Trim();
        Description = description?.Trim();
        Date = date ?? DateTimeOffset.MinValue;
    }
}
