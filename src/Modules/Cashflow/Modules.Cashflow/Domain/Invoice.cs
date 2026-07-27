using FSH.Framework.Core.Domain;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>
/// A fiscal document — issued to a client (<see cref="InvoiceType.Emitida"/>) or received from a
/// supplier (<see cref="InvoiceType.Recibida"/>). One invoice fans out to N cash lines
/// (<c>Income</c>/<c>Payment</c>) that carry its <c>InvoiceId</c>; the invoice never navigates to
/// them (each is its own aggregate root — referenced by identity, §2). Counterparty / catalog
/// references (Client, Supplier, Company, Society, Status) are soft references — indexed Guid
/// columns, not DB foreign keys — consistent with <see cref="Project"/>.
/// </summary>
public sealed class Invoice : AggregateRoot<Guid>
{
    public string Number { get; private set; } = default!;
    public string? DynamicsNumber { get; private set; }
    public InvoiceType Type { get; private set; }

    /// <summary>Stored at midnight (Unspecified kind) so it never shifts a day — like Income/Payment.</summary>
    public DateTime? InvoiceDate { get; private set; }
    public DateTime? DueDate { get; private set; }

    /// <summary>Set when <see cref="Type"/> is <see cref="InvoiceType.Emitida"/> (soft ref → Clients).</summary>
    public Guid? ClientId { get; private set; }

    /// <summary>Set when <see cref="Type"/> is <see cref="InvoiceType.Recibida"/> (soft ref → Suppliers).</summary>
    public Guid? SupplierId { get; private set; }

    /// <summary>Group legal entity that issues/receives (soft ref → Companies).</summary>
    public Guid? CompanyId { get; private set; }

    /// <summary>Billable society of the client — issued invoices only (soft ref → Societies).</summary>
    public Guid? SocietyId { get; private set; }

    /// <summary>Project this invoice belongs to (soft ref → Proyectos). Optional — overhead
    /// invoices have none. Cash lines created from the invoice inherit it.</summary>
    public Guid? ProjectId { get; private set; }

    public decimal? TaxBase { get; private set; }
    public decimal? Vat { get; private set; }

    /// <summary>Gross total. May be negative for credit notes / rectificativas (§8).</summary>
    public decimal Total { get; private set; }

    /// <summary>Payment-terms value object; persisted as its code string (§5). Null when unknown.</summary>
    public PaymentTerms? PaymentTerms { get; private set; }

    /// <summary>Source bank (received SOLUTIONS schema only) — free text.</summary>
    public string? Bank { get; private set; }

    /// <summary>Soft reference to the Status catalog (indexed, no FK).</summary>
    public Guid? StatusId { get; private set; }

    /// <summary>"COMPROBADO CON LISTADO" — reconciled against the master listing (§3).</summary>
    public bool Verified { get; private set; }

    public string? Notes { get; private set; }

    /// <summary>Storage key of the attached source document (PDF/image the invoice was created
    /// from). Nullable — most legacy-loaded invoices have none.</summary>
    public string? DocumentPath { get; private set; }

    private Invoice() { }

    /// <summary>Creates an issued (Emitida) invoice — one tied to a client, driving Incomes.</summary>
    public static Invoice Issued(
        string number,
        Guid clientId,
        decimal total,
        DateTime? invoiceDate = null,
        DateTime? dueDate = null,
        Guid? companyId = null,
        Guid? societyId = null,
        decimal? taxBase = null,
        decimal? vat = null,
        string? paymentTerms = null,
        Guid? statusId = null,
        string? dynamicsNumber = null,
        string? notes = null,
        Guid? projectId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        return new Invoice
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            Type = InvoiceType.Emitida,
            ClientId = clientId,
            ProjectId = projectId,
            Total = total,
            InvoiceDate = NormalizeDate(invoiceDate),
            DueDate = NormalizeDate(dueDate),
            CompanyId = companyId,
            SocietyId = societyId,
            TaxBase = taxBase,
            Vat = vat,
            PaymentTerms = ParseTerms(paymentTerms),
            StatusId = statusId,
            DynamicsNumber = dynamicsNumber?.Trim(),
            Notes = notes?.Trim(),
        };
    }

    /// <summary>Creates a received (Recibida) invoice — one tied to a supplier, driving Payments.</summary>
    public static Invoice Received(
        string number,
        Guid supplierId,
        decimal total,
        DateTime? invoiceDate = null,
        DateTime? dueDate = null,
        Guid? companyId = null,
        decimal? taxBase = null,
        decimal? vat = null,
        string? paymentTerms = null,
        string? bank = null,
        Guid? statusId = null,
        string? dynamicsNumber = null,
        string? notes = null,
        Guid? projectId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        return new Invoice
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            Type = InvoiceType.Recibida,
            SupplierId = supplierId,
            ProjectId = projectId,
            Total = total,
            InvoiceDate = NormalizeDate(invoiceDate),
            DueDate = NormalizeDate(dueDate),
            CompanyId = companyId,
            TaxBase = taxBase,
            Vat = vat,
            PaymentTerms = ParseTerms(paymentTerms),
            Bank = bank?.Trim(),
            StatusId = statusId,
            DynamicsNumber = dynamicsNumber?.Trim(),
            Notes = notes?.Trim(),
        };
    }

    /// <summary>Updates the editable header fields. <see cref="Type"/> and the counterparty side are
    /// immutable once created — a mistyped side is deleted and recreated, not mutated.</summary>
    public void Update(
        string number,
        decimal total,
        DateTime? invoiceDate,
        DateTime? dueDate,
        Guid? clientId,
        Guid? supplierId,
        Guid? companyId,
        Guid? societyId,
        decimal? taxBase,
        decimal? vat,
        string? paymentTerms,
        string? bank,
        Guid? statusId,
        string? dynamicsNumber,
        string? notes,
        Guid? projectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number.Trim();
        Total = total;
        InvoiceDate = NormalizeDate(invoiceDate);
        DueDate = NormalizeDate(dueDate);
        ClientId = clientId;
        SupplierId = supplierId;
        CompanyId = companyId;
        SocietyId = societyId;
        ProjectId = projectId;
        TaxBase = taxBase;
        Vat = vat;
        PaymentTerms = ParseTerms(paymentTerms);
        Bank = bank?.Trim();
        StatusId = statusId;
        DynamicsNumber = dynamicsNumber?.Trim();
        Notes = notes?.Trim();
    }

    public void MarkVerified() => Verified = true;

    public void SetStatus(Guid? statusId) => StatusId = statusId;

    /// <summary>Attaches (or clears) the source document. Kept off <see cref="Update"/> so a routine
    /// edit never silently detaches the file.</summary>
    public void AttachDocument(string? documentPath) => DocumentPath = documentPath;

    private static PaymentTerms? ParseTerms(string? raw) =>
        Domain.PaymentTerms.TryParse(raw, out var terms) ? terms : null;

    // Midnight + Unspecified kind: no timezone offset, so serialization never bumps the day.
    private static DateTime? NormalizeDate(DateTime? date) =>
        date is null ? null : DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Unspecified);
}
