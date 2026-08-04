using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Services;

/// <summary>Previous record's identity — the AEAT chain block (Encadenamiento/RegistroAnterior)
/// carries it alongside the previous Huella.</summary>
public sealed record AeatPreviousRecordRef(string EmitterNif, string InvoiceNumber, DateTime IssueDate, string Hash);

/// <summary>One "registro de alta" to submit, fully resolved (no data access in the client).</summary>
public sealed record AeatAltaRecord(
    string InvoiceNumber,
    DateTime IssueDate,
    string Description,
    string? RecipientName,
    string? RecipientNif,
    decimal TaxBase,
    decimal? TaxRate,
    decimal VatAmount,
    decimal TotalAmount,
    AeatPreviousRecordRef? Previous,
    DateTimeOffset GeneratedAt,
    string Hash);

public sealed record AeatSubmission(
    VerifactuEnvironment Environment,
    byte[] PfxBytes,
    string PfxPassword,
    string EmitterName,
    string EmitterNif,
    string SoftwareName,
    string SoftwareId,
    string SoftwareVersion,
    string InstallationNumber,
    IReadOnlyList<AeatAltaRecord> Records);

public sealed record AeatRecordResult(string InvoiceNumber, VerifactuStatus Status, string? Code, string? Description);

public sealed record AeatSubmissionResult(
    string RequestXml,
    string ResponseXml,
    string? GlobalStatus,
    int? WaitSeconds,
    IReadOnlyList<AeatRecordResult> Records);

public interface IAeatVerifactuClient
{
    Task<AeatSubmissionResult> SubmitAsync(AeatSubmission submission, CancellationToken cancellationToken = default);
}

/// <summary>
/// SOAP client for the AEAT VERI*FACTU submission webservice (RegFactuSistemaFacturacion,
/// "Veri-Factu_Descripcion_SWeb" v1.0.3). The HttpClient is built PER CALL because the client-TLS
/// certificate differs per Company — a shared named client cannot carry per-company certs. The PFX
/// lives only in memory, never on disk.
///
/// ⚠️ Verificación pendiente (plan §21): la estructura del XML sigue los XSD publicados
/// (SuministroLR.xsd / SuministroInformacion.xsd) según la documentación oficial, pero DEBE
/// contrastarse end-to-end contra el entorno de pruebas de la AEAT con un certificado real antes
/// de activar producción.
/// </summary>
public sealed class AeatVerifactuClient : IAeatVerifactuClient
{
    // Endpoints oficiales (Anexo I del documento del servicio web, v1.0.3).
    private const string EndpointProduccion = "https://www1.agenciatributaria.gob.es/wlpl/TIKE-CONT/ws/SistemaFacturacion/VerifactuSOAP";
    private const string EndpointPruebas = "https://prewww1.aeat.es/wlpl/TIKE-CONT/ws/SistemaFacturacion/VerifactuSOAP";

    private static readonly XNamespace SoapNs = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace SumNs = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroLR.xsd";
    private static readonly XNamespace SfNs = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/tike/cont/ws/SuministroInformacion.xsd";

    public async Task<AeatSubmissionResult> SubmitAsync(AeatSubmission submission, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(submission);
        if (submission.Records.Count == 0)
        {
            throw new ArgumentException("Nothing to submit.", nameof(submission));
        }

        string requestXml = BuildEnvelope(submission);

        using var certificate = X509CertificateLoader.LoadPkcs12(submission.PfxBytes, submission.PfxPassword);
        using var handler = new SocketsHttpHandler();
        handler.SslOptions.ClientCertificates = new X509CertificateCollection { certificate };
        using var http = new HttpClient(handler, disposeHandler: false);
        http.Timeout = TimeSpan.FromSeconds(60);

        string endpoint = submission.Environment == VerifactuEnvironment.Produccion ? EndpointProduccion : EndpointPruebas;
        using var content = new StringContent(requestXml, Encoding.UTF8, "text/xml");
        content.Headers.Add("SOAPAction", "\"\"");

        using var response = await http.PostAsync(new Uri(endpoint), content, cancellationToken).ConfigureAwait(false);
        string responseXml = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return ParseResponse(requestXml, responseXml, submission.Records);
    }

    private static string BuildEnvelope(AeatSubmission s)
    {
        var registros = s.Records.Select(r => BuildRegistroFactura(s, r));

        var envelope = new XElement(SoapNs + "Envelope",
            new XAttribute(XNamespace.Xmlns + "soapenv", SoapNs),
            new XAttribute(XNamespace.Xmlns + "sum", SumNs),
            new XAttribute(XNamespace.Xmlns + "sf", SfNs),
            new XElement(SoapNs + "Header"),
            new XElement(SoapNs + "Body",
                new XElement(SumNs + "RegFactuSistemaFacturacion",
                    new XElement(SumNs + "Cabecera",
                        new XElement(SfNs + "ObligadoEmision",
                            new XElement(SfNs + "NombreRazon", s.EmitterName),
                            new XElement(SfNs + "NIF", s.EmitterNif))),
                    registros)));

        return new XDocument(new XDeclaration("1.0", "utf-8", null), envelope).ToString(SaveOptions.DisableFormatting);
    }

    private static XElement BuildRegistroFactura(AeatSubmission s, AeatAltaRecord r)
    {
        XElement encadenamiento = r.Previous is null
            ? new XElement(SfNs + "Encadenamiento", new XElement(SfNs + "PrimerRegistro", "S"))
            : new XElement(SfNs + "Encadenamiento",
                new XElement(SfNs + "RegistroAnterior",
                    new XElement(SfNs + "IDEmisorFactura", r.Previous.EmitterNif),
                    new XElement(SfNs + "NumSerieFactura", r.Previous.InvoiceNumber),
                    new XElement(SfNs + "FechaExpedicionFactura", FormatDate(r.Previous.IssueDate)),
                    new XElement(SfNs + "Huella", r.Previous.Hash)));

        var alta = new XElement(SfNs + "RegistroAlta",
            new XElement(SfNs + "IDVersion", "1.0"),
            new XElement(SfNs + "IDFactura",
                new XElement(SfNs + "IDEmisorFactura", s.EmitterNif),
                new XElement(SfNs + "NumSerieFactura", r.InvoiceNumber),
                new XElement(SfNs + "FechaExpedicionFactura", FormatDate(r.IssueDate))),
            new XElement(SfNs + "NombreRazonEmisor", s.EmitterName),
            new XElement(SfNs + "TipoFactura", "F1"),
            new XElement(SfNs + "DescripcionOperacion", r.Description));

        if (!string.IsNullOrWhiteSpace(r.RecipientNif))
        {
            alta.Add(new XElement(SfNs + "Destinatarios",
                new XElement(SfNs + "IDDestinatario",
                    new XElement(SfNs + "NombreRazon", r.RecipientName ?? r.RecipientNif),
                    new XElement(SfNs + "NIF", r.RecipientNif))));
        }

        alta.Add(
            new XElement(SfNs + "Desglose",
                new XElement(SfNs + "DetalleDesglose",
                    new XElement(SfNs + "Impuesto", "01"),           // IVA
                    new XElement(SfNs + "ClaveRegimen", "01"),       // régimen general
                    new XElement(SfNs + "CalificacionOperacion", "S1"), // sujeta y no exenta
                    new XElement(SfNs + "TipoImpositivo", FormatAmount(r.TaxRate ?? 21m)),
                    new XElement(SfNs + "BaseImponibleOimporteNoSujeto", FormatAmount(r.TaxBase)),
                    new XElement(SfNs + "CuotaRepercutida", FormatAmount(r.VatAmount)))),
            new XElement(SfNs + "CuotaTotal", FormatAmount(r.VatAmount)),
            new XElement(SfNs + "ImporteTotal", FormatAmount(r.TotalAmount)),
            encadenamiento,
            new XElement(SfNs + "SistemaInformatico",
                new XElement(SfNs + "NombreRazon", s.EmitterName),
                new XElement(SfNs + "NIF", s.EmitterNif),
                new XElement(SfNs + "NombreSistemaInformatico", s.SoftwareName),
                new XElement(SfNs + "IdSistemaInformatico", s.SoftwareId),
                new XElement(SfNs + "Version", s.SoftwareVersion),
                new XElement(SfNs + "NumeroInstalacion", s.InstallationNumber),
                new XElement(SfNs + "TipoUsoPosibleSoloVerifactu", "S"),
                new XElement(SfNs + "TipoUsoPosibleMultiOT", "S"),
                new XElement(SfNs + "IndicadorMultiplesOT", "S")),
            new XElement(SfNs + "FechaHoraHusoGenRegistro", r.GeneratedAt.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture)),
            new XElement(SfNs + "TipoHuella", "01"),                 // SHA-256 (lista L12)
            new XElement(SfNs + "Huella", r.Hash));

        return new XElement(SumNs + "RegistroFactura", alta);
    }

    private static AeatSubmissionResult ParseResponse(string requestXml, string responseXml, IReadOnlyList<AeatAltaRecord> sent)
    {
        var doc = XDocument.Parse(responseXml);

        // Namespace-agnostic reads: the response uses the RespuestaSuministro.xsd namespace, but
        // matching by local name keeps the parser stable across minor AEAT revisions.
        string? globalStatus = FindValue(doc, "EstadoEnvio");
        int? wait = int.TryParse(FindValue(doc, "TiempoEsperaEnvio"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int w) ? w : null;

        var lines = new List<AeatRecordResult>();
        foreach (var linea in doc.Descendants().Where(e => e.Name.LocalName == "RespuestaLinea"))
        {
            string? numSerie = linea.Descendants().FirstOrDefault(e => e.Name.LocalName == "NumSerieFactura")?.Value;
            string? estado = linea.Descendants().FirstOrDefault(e => e.Name.LocalName == "EstadoRegistro")?.Value;
            string? codigo = linea.Descendants().FirstOrDefault(e => e.Name.LocalName == "CodigoErrorRegistro")?.Value;
            string? descripcion = linea.Descendants().FirstOrDefault(e => e.Name.LocalName == "DescripcionErrorRegistro")?.Value;
            lines.Add(new AeatRecordResult(numSerie ?? string.Empty, MapEstado(estado), codigo, descripcion));
        }

        // A SoapFault or an empty body yields no per-record lines — the caller treats that as
        // a technical error for every record it sent.
        if (lines.Count == 0 && sent.Count > 0 && globalStatus is null)
        {
            lines.AddRange(sent.Select(r => new AeatRecordResult(r.InvoiceNumber, VerifactuStatus.ErrorTecnico, null, "Respuesta sin líneas (posible SoapFault)")));
        }

        return new AeatSubmissionResult(requestXml, responseXml, globalStatus, wait, lines);
    }

    private static string? FindValue(XDocument doc, string localName) =>
        doc.Descendants().FirstOrDefault(e => e.Name.LocalName == localName)?.Value;

    private static VerifactuStatus MapEstado(string? estado) => estado switch
    {
        "Correcto" => VerifactuStatus.Aceptada,
        "AceptadoConErrores" => VerifactuStatus.AceptadaConErrores,
        "Incorrecto" => VerifactuStatus.Rechazada,
        _ => VerifactuStatus.ErrorTecnico,
    };

    private static string FormatDate(DateTime date) => date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);

    private static string FormatAmount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
