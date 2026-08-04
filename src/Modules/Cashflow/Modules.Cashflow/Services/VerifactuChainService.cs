using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Services;

/// <summary>Everything the chained record needs, already resolved — the service does no data access
/// and no clock reads, so identical input always yields an identical hash (unit-testable).</summary>
public sealed record VerifactuChainInput(
    string EmitterNif,
    string InvoiceNumber,
    DateTime IssueDate,
    decimal VatAmount,
    decimal TotalAmount,
    string? PreviousHash,
    DateTimeOffset GeneratedAt);

public sealed record VerifactuChainResult(string Hash, string CanonicalInput, string QrPayload);

public interface IVerifactuChainService
{
    VerifactuChainResult BuildRecord(VerifactuChainInput input, VerifactuEnvironment environment);
}

/// <summary>
/// Builds the VERI*FACTU "registro de facturación de alta": the canonical field string, its
/// chained SHA-256 huella, and the QR cotejo URL — per Orden HAC/1177/2024 (RD 1007/2023).
///
/// Formato de la huella (detalle técnico del Anexo / documento de especificaciones de la AEAT):
/// concatenación "campo=valor" separada por '&amp;', valores sin espacios envolventes, codificada
/// en UTF-8, SHA-256, resultado hexadecimal en MAYÚSCULAS (64 chars). Para el primer registro de
/// la cadena, Huella se incluye con valor vacío. Campos y orden:
///   IDEmisorFactura, NumSerieFactura, FechaExpedicionFactura (dd-mm-aaaa), TipoFactura,
///   CuotaTotal, ImporteTotal, Huella (registro anterior), FechaHoraHusoGenRegistro (ISO 8601 con huso).
/// Los importes van con punto decimal y dos decimales (invariant).
/// </summary>
public sealed class VerifactuChainService : IVerifactuChainService
{
    // Factura completa (F1). Simplificadas (F2) y rectificativas (R*) quedan fuera de alcance.
    private const string TipoFactura = "F1";

    // Servicio de cotejo del QR — producción y entorno de pruebas de la AEAT.
    private const string QrBaseProduccion = "https://www2.agenciatributaria.gob.es/wlpl/TIKE-CONT/ValidarQR";
    private const string QrBasePruebas = "https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR";

    public VerifactuChainResult BuildRecord(VerifactuChainInput input, VerifactuEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.EmitterNif);
        ArgumentException.ThrowIfNullOrWhiteSpace(input.InvoiceNumber);

        string fechaExpedicion = input.IssueDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        string cuota = FormatAmount(input.VatAmount);
        string importe = FormatAmount(input.TotalAmount);
        string fechaHora = input.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        string canonical =
            $"IDEmisorFactura={input.EmitterNif.Trim()}" +
            $"&NumSerieFactura={input.InvoiceNumber.Trim()}" +
            $"&FechaExpedicionFactura={fechaExpedicion}" +
            $"&TipoFactura={TipoFactura}" +
            $"&CuotaTotal={cuota}" +
            $"&ImporteTotal={importe}" +
            $"&Huella={input.PreviousHash?.Trim() ?? string.Empty}" +
            $"&FechaHoraHusoGenRegistro={fechaHora}";

        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));

        string qrBase = environment == VerifactuEnvironment.Produccion ? QrBaseProduccion : QrBasePruebas;
        string qrPayload =
            $"{qrBase}?nif={Uri.EscapeDataString(input.EmitterNif.Trim())}" +
            $"&numserie={Uri.EscapeDataString(input.InvoiceNumber.Trim())}" +
            $"&fecha={Uri.EscapeDataString(fechaExpedicion)}" +
            $"&importe={Uri.EscapeDataString(importe)}";

        return new VerifactuChainResult(hash, canonical, qrPayload);
    }

    private static string FormatAmount(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);
}
