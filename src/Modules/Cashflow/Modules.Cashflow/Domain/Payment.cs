using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Payment : AggregateRoot<Guid>
{
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    /// <summary>Day of the payment, stored at midnight (Unspecified kind) so it never shifts a day.</summary>
    public DateTime? Date { get; private set; }
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

    /// <summary>Creates a payment. Respects the <paramref name="confirmed"/> flag (does not force false).</summary>
    public static Payment Create(
        decimal amount,
        string? description,
        DateTime? date,
        decimal? percentage,
        Guid? supplierId,
        Guid? projectId,
        Guid? statusId,
        bool confirmed)
    {
        return new Payment
        {
            Id = Guid.CreateVersion7(),
            Amount = amount,
            Description = description?.Trim(),
            Date = NormalizeDate(date),
            Percentage = percentage,
            SupplierId = supplierId,
            ProjectId = projectId,
            StatusId = statusId,
            Confirmed = confirmed,
            Validated = false,
        };
    }

    /// <summary>
    /// Updates the editable fields. Deliberately does NOT touch <see cref="Confirmed"/> or
    /// <see cref="Validated"/> — those change only via <see cref="SetConfirmed"/> / <see cref="SetValidated"/>.
    /// </summary>
    public void Update(
        decimal amount,
        string? description,
        DateTime? date,
        decimal? percentage,
        Guid? supplierId,
        Guid? projectId,
        Guid? statusId)
    {
        Amount = amount;
        Description = description?.Trim();
        Date = NormalizeDate(date);
        Percentage = percentage;
        SupplierId = supplierId;
        ProjectId = projectId;
        StatusId = statusId;
    }

    public void SetConfirmed(bool value) => Confirmed = value;

    public void SetValidated(bool value) => Validated = value;

    // Midnight + Unspecified kind: no timezone offset, so serialization never bumps the day.
    private static DateTime? NormalizeDate(DateTime? date) =>
        date is null ? null : DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Unspecified);
}
