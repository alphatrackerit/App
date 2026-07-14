using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Payment : AggregateRoot<Guid>
{
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset? Date { get; private set; }
    public decimal? Percentage { get; private set; }

    /// <summary>Soft reference to the external Proveedores catalog (indexed, no FK).</summary>
    public Guid? SupplierId { get; private set; }

    /// <summary>Owning project (same module → real FK, ON DELETE NO ACTION). Nullable per spec.</summary>
    public Guid? ProjectId { get; private set; }

    /// <summary>Soft reference to the external Estados catalog (indexed, no FK).</summary>
    public Guid? StatusId { get; private set; }

    public bool Confirmed { get; private set; }
    public bool Validated { get; private set; }

    private Payment() { }

    public static Payment Create(
        decimal amount,
        string? description,
        DateTimeOffset? date,
        decimal? percentage,
        Guid? supplierId,
        Guid? projectId,
        Guid? statusId)
    {
        return new Payment
        {
            Id = Guid.CreateVersion7(),
            Amount = amount,
            Description = description?.Trim(),
            Date = date,
            Percentage = percentage,
            SupplierId = supplierId,
            ProjectId = projectId,
            StatusId = statusId,
            Confirmed = false,
            Validated = false,
        };
    }

    public void Update(
        decimal amount,
        string? description,
        DateTimeOffset? date,
        decimal? percentage,
        Guid? supplierId,
        Guid? projectId,
        Guid? statusId)
    {
        Amount = amount;
        Description = description?.Trim();
        Date = date;
        Percentage = percentage;
        SupplierId = supplierId;
        ProjectId = projectId;
        StatusId = statusId;
    }

    public void SetConfirmed(bool value) => Confirmed = value;

    public void SetValidated(bool value) => Validated = value;
}
