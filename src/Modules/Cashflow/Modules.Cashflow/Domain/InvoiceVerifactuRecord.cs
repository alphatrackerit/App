using FSH.Framework.Core.Domain;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>
/// The AEAT "registro de facturación de alta" of an issued invoice (1:1, real FK): the chained
/// SHA-256 hash, the QR payload and the submission state. Immutable once accepted — a wrong
/// invoice is fixed with a rectificativa (future work), never by editing this record.
/// </summary>
public sealed class InvoiceVerifactuRecord : AggregateRoot<Guid>
{
    public Guid InvoiceId { get; private set; }

    /// <summary>Hash of the previous record in this company's chain; null for the first record.</summary>
    public string? PreviousHash { get; private set; }

    /// <summary>SHA-256 (hex, uppercase) of this record's canonical string.</summary>
    public string Hash { get; private set; } = default!;

    /// <summary>FechaHoraHusoGenRegistro — generation timestamp WITH offset; part of the hashed input.</summary>
    public DateTimeOffset GeneratedAt { get; private set; }

    /// <summary>Full URL encoded in the invoice's QR (AEAT cotejo service).</summary>
    public string QrPayload { get; private set; } = default!;

    public VerifactuStatus Status { get; private set; } = VerifactuStatus.PendienteEnvio;

    public string? AeatResponseCode { get; private set; }

    // Auditoría: request/response crudos del envío SOAP (Fase 2).
    public string? RawRequestXml { get; private set; }
    public string? RawResponseXml { get; private set; }

    public int RetryCount { get; private set; }

    private InvoiceVerifactuRecord() { }

    public static InvoiceVerifactuRecord Create(
        Guid invoiceId, string? previousHash, string hash, DateTimeOffset generatedAt, string qrPayload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        ArgumentException.ThrowIfNullOrWhiteSpace(qrPayload);
        return new InvoiceVerifactuRecord
        {
            Id = Guid.CreateVersion7(),
            InvoiceId = invoiceId,
            PreviousHash = previousHash,
            Hash = hash,
            GeneratedAt = generatedAt,
            QrPayload = qrPayload,
        };
    }

    public void MarkSent(string rawRequestXml)
    {
        Status = VerifactuStatus.Enviada;
        RawRequestXml = rawRequestXml;
    }

    public void MarkResult(VerifactuStatus status, string? aeatResponseCode, string? rawResponseXml)
    {
        Status = status;
        AeatResponseCode = aeatResponseCode;
        RawResponseXml = rawResponseXml;
    }

    public void MarkTechnicalError()
    {
        Status = VerifactuStatus.ErrorTecnico;
        RetryCount++;
    }
}
