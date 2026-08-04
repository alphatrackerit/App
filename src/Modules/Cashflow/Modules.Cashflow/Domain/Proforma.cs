using FSH.Framework.Core.Domain;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>
/// A commercial pre-invoice document — issued to a client (<see cref="InvoiceType.Emitida"/>) or
/// received from a supplier (<see cref="InvoiceType.Recibida"/>). One proforma fans out to N
/// <see cref="Invoice"/>s that carry its <c>ProformaId</c>; the proforma never navigates to them
/// (each is its own aggregate root — referenced by identity, same design as Invoice→Income/Payment).
/// A proforma is NOT a fiscal document: it never enters the VeriFactu engine — only real invoices do.
/// Counterparty / catalog references are soft references, consistent with <see cref="Invoice"/>.
/// </summary>
public sealed class Proforma : AggregateRoot<Guid>
{
    public string Number { get; private set; } = default!;

    /// <summary>Reuses <see cref="InvoiceType"/> — identical semantics, no parallel enum.</summary>
    public InvoiceType Type { get; private set; }

    /// <summary>Stored at midnight (Unspecified kind) so it never shifts a day — like Invoice.</summary>
    public DateTime? Date { get; private set; }

    /// <summary>Set when <see cref="Type"/> is <see cref="InvoiceType.Emitida"/> (soft ref → Clients).</summary>
    public Guid? ClientId { get; private set; }

    /// <summary>Set when <see cref="Type"/> is <see cref="InvoiceType.Recibida"/> (soft ref → Suppliers).</summary>
    public Guid? SupplierId { get; private set; }

    /// <summary>Group legal entity that issues/receives (soft ref → Companies).</summary>
    public Guid? CompanyId { get; private set; }

    /// <summary>Billable society of the client — issued proformas only (soft ref → Societies).</summary>
    public Guid? SocietyId { get; private set; }

    /// <summary>Project this proforma belongs to (soft ref → Proyectos). Optional and independent
    /// of <c>Project.SalePrice</c> — parallel concepts.</summary>
    public Guid? ProjectId { get; private set; }

    public decimal? TaxBase { get; private set; }
    public decimal? Vat { get; private set; }

    /// <summary>Gross total. The cuadre against linked invoices is informative only.</summary>
    public decimal Total { get; private set; }

    /// <summary>Payment-terms value object; persisted as its code string. Drives
    /// invoice generation (1 milestone → 1 draft invoice). Null when unknown.</summary>
    public PaymentTerms? PaymentTerms { get; private set; }

    /// <summary>Soft reference to the Status catalog (ProformaEmitida/ProformaRecibida statuses).</summary>
    public Guid? StatusId { get; private set; }

    /// <summary>Person in charge of this proforma — free text (no user reference; the source
    /// listings carry plain names).</summary>
    public string? Responsible { get; private set; }

    public string? Notes { get; private set; }

    /// <summary>Storage key of the attached source document.</summary>
    public string? DocumentPath { get; private set; }

    private Proforma() { }

    /// <summary>Creates an issued (Emitida) proforma — one tied to a client.</summary>
    public static Proforma Issued(
        string number,
        Guid clientId,
        decimal total,
        DateTime? date = null,
        Guid? companyId = null,
        Guid? societyId = null,
        Guid? projectId = null,
        decimal? taxBase = null,
        decimal? vat = null,
        string? paymentTerms = null,
        Guid? statusId = null,
        string? notes = null,
        string? responsible = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        return new Proforma
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            Type = InvoiceType.Emitida,
            ClientId = clientId,
            Total = total,
            Date = NormalizeDate(date),
            CompanyId = companyId,
            SocietyId = societyId,
            ProjectId = projectId,
            TaxBase = taxBase,
            Vat = vat,
            PaymentTerms = ParseTerms(paymentTerms),
            StatusId = statusId,
            Responsible = responsible?.Trim(),
            Notes = notes?.Trim(),
        };
    }

    /// <summary>Creates a received (Recibida) proforma — one tied to a supplier.</summary>
    public static Proforma Received(
        string number,
        Guid supplierId,
        decimal total,
        DateTime? date = null,
        Guid? companyId = null,
        Guid? projectId = null,
        decimal? taxBase = null,
        decimal? vat = null,
        string? paymentTerms = null,
        Guid? statusId = null,
        string? notes = null,
        string? responsible = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        return new Proforma
        {
            Id = Guid.CreateVersion7(),
            Number = number.Trim(),
            Type = InvoiceType.Recibida,
            SupplierId = supplierId,
            Total = total,
            Date = NormalizeDate(date),
            CompanyId = companyId,
            ProjectId = projectId,
            TaxBase = taxBase,
            Vat = vat,
            PaymentTerms = ParseTerms(paymentTerms),
            StatusId = statusId,
            Responsible = responsible?.Trim(),
            Notes = notes?.Trim(),
        };
    }

    /// <summary>Updates the editable header fields. <see cref="Type"/> is immutable once created —
    /// a mistyped side is deleted and recreated, not mutated (same rule as Invoice).</summary>
    public void Update(
        string number,
        decimal total,
        DateTime? date,
        Guid? clientId,
        Guid? supplierId,
        Guid? companyId,
        Guid? societyId,
        Guid? projectId,
        decimal? taxBase,
        decimal? vat,
        string? paymentTerms,
        Guid? statusId,
        string? notes,
        string? responsible)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(number);
        Number = number.Trim();
        Total = total;
        Date = NormalizeDate(date);
        ClientId = clientId;
        SupplierId = supplierId;
        CompanyId = companyId;
        SocietyId = societyId;
        ProjectId = projectId;
        TaxBase = taxBase;
        Vat = vat;
        PaymentTerms = ParseTerms(paymentTerms);
        StatusId = statusId;
        Responsible = responsible?.Trim();
        Notes = notes?.Trim();
    }

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
