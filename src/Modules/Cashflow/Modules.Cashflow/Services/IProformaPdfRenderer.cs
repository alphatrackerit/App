namespace FSH.Modules.Cashflow.Services;

/// <summary>Everything the PDF needs, already resolved — the renderer does no data access.</summary>
public sealed record ProformaPdfData(
    string Number,
    string TypeLabel,
    DateTime? Date,
    string CounterpartyLabel,
    string? CounterpartyName,
    string? CompanyName,
    decimal? TaxBase,
    decimal? Vat,
    decimal Total,
    string? PaymentTermsCode,
    string? StatusName,
    string? Notes,
    IReadOnlyList<(decimal Amount, DateTime? DueDate, decimal Percentage)> Milestones);

/// <summary>Renders a proforma as a commercial PDF document. No QR/hash — a proforma is not a
/// fiscal document and never enters the VeriFactu engine.</summary>
public interface IProformaPdfRenderer
{
    byte[] Render(ProformaPdfData data);
}
