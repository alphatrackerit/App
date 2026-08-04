using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>
/// One line/concept of an <see cref="Invoice"/> ("Conceptos" — presentable detail for the PDF).
/// Child of the Invoice aggregate: reached only through <see cref="Invoice.Items"/>, replaced as a
/// whole via <see cref="Invoice.SetItems"/>, never queried standalone. Purely presentational —
/// the fiscal amounts remain the invoice header's TaxBase/Vat/Total.
/// </summary>
public sealed class InvoiceItem : BaseEntity<Guid>
{
    public Guid InvoiceId { get; private set; }
    public int Position { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>Line amount (= Quantity × UnitPrice, rounded to cents), stored for the PDF.</summary>
    public decimal Amount { get; private set; }

    private InvoiceItem() { }

    internal static InvoiceItem Create(Guid invoiceId, int position, string description, decimal quantity, decimal unitPrice)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        return new InvoiceItem
        {
            Id = Guid.CreateVersion7(),
            InvoiceId = invoiceId,
            Position = position,
            Description = description.Trim(),
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero),
        };
    }
}
