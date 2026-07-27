using System.Net;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Storage;
using FSH.Framework.Storage;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StorageFileType = FSH.Framework.Storage.FileType;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.ExtractInvoice;

public sealed partial class ExtractInvoiceCommandHandler(
    CashflowDbContext dbContext,
    IStorageService storage,
    IOptions<CashflowAiOptions> aiOptions,
    ILogger<ExtractInvoiceCommandHandler> logger)
    : ICommandHandler<ExtractInvoiceCommand, InvoiceExtractionDto>
{
    public async ValueTask<InvoiceExtractionDto> Handle(ExtractInvoiceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        string? apiKey = aiOptions.Value.ApiKey
            ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new CustomException(
                "La extracción con IA no está configurada (falta CashflowAi__ApiKey).",
                Array.Empty<string>(), HttpStatusCode.ServiceUnavailable);
        }

        // 1. Persist the document first — it survives even if the model call fails, and the user
        //    attaches it to the invoice they confirm.
        bool isPdf = string.Equals(command.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
        string documentPath = await storage.UploadAsync<Domain.Invoice>(
            new FileUploadRequest
            {
                FileName = command.FileName,
                ContentType = command.ContentType,
                Data = [.. command.Content],
            },
            isPdf ? StorageFileType.Pdf : StorageFileType.Image,
            cancellationToken).ConfigureAwait(false);

        // 2. Group companies help the model decide Emitida (we issued it) vs Recibida (we received it).
        var companyNames = await dbContext.Companies.AsNoTracking()
            .Select(c => c.Name)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        // 3. Vision call with structured output — the response is guaranteed to match the schema.
        string json = await CallModelAsync(apiKey, command, companyNames, cancellationToken).ConfigureAwait(false);
        LogExtractionResult(logger, command.FileName, json.Length);

        var proposal = ParseProposal(json, documentPath);

        // 4. Resolve the counterparty name against the catalog (normalized match) so the form can
        //    pre-select the client/supplier combobox.
        return await ResolveCounterpartyAsync(proposal, cancellationToken).ConfigureAwait(false);
    }

    private async Task<string> CallModelAsync(
        string apiKey, ExtractInvoiceCommand command, List<string> companyNames, CancellationToken cancellationToken)
    {
        using AnthropicClient client = new() { ApiKey = apiKey };

        string base64 = Convert.ToBase64String(command.Content);
        bool isPdf = string.Equals(command.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);

        ContentBlockParam fileBlock = isPdf
            ? new DocumentBlockParam { Source = new Base64PdfSource { Data = base64 } }
            : new ImageBlockParam { Source = new Base64ImageSource { Data = base64, MediaType = command.ContentType } };

        string prompt = BuildPrompt(companyNames);

        Message response;
        try
        {
            response = await client.Messages.Create(
                new MessageCreateParams
                {
                    Model = aiOptions.Value.Model,
                    MaxTokens = aiOptions.Value.MaxOutputTokens,
                    Thinking = new ThinkingConfigAdaptive(),
                    OutputConfig = new OutputConfig { Format = BuildOutputFormat() },
                    Messages =
                    [
                        new()
                        {
                            Role = Role.User,
                            Content = new List<ContentBlockParam>
                            {
                                fileBlock,
                                new TextBlockParam { Text = prompt },
                            },
                        },
                    ],
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogModelCallFailed(logger, ex);
            throw new CustomException(
                "El modelo de IA no pudo procesar el documento. Inténtalo de nuevo o rellena los campos a mano.",
                Array.Empty<string>(), HttpStatusCode.BadGateway);
        }

        string? json = response.Content
            .Select(b => b.Value)
            .OfType<TextBlock>()
            .Select(t => t.Text)
            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));

        return json ?? throw new CustomException(
            "El modelo no devolvió datos para este documento.", Array.Empty<string>(), HttpStatusCode.BadGateway);
    }

    private static string BuildPrompt(List<string> companyNames)
    {
        string companies = companyNames.Count > 0
            ? string.Join(", ", companyNames)
            : "(sin catálogo)";
        return $$"""
            Extrae los datos de esta factura. Nuestras empresas del grupo son: {{companies}}.
            Reglas:
            - "type": "Emitida" si la factura la EMITE una de nuestras empresas (el cliente es un tercero);
              "Recibida" si una de nuestras empresas la RECIBE de un proveedor; null si no está claro.
            - "counterpartyName": el nombre del tercero (el cliente si es Emitida, el proveedor si es Recibida),
              NUNCA el de nuestra empresa.
            - Fechas en formato YYYY-MM-DD. Importes como números (punto decimal), sin símbolo de moneda.
            - "total" es el total con impuestos; "taxBase" la base imponible; "vat" la cuota de IVA.
            - "paymentTerms": SOLO uno de estos códigos si se deduce de las condiciones de pago:
              0D, 30D, 60D, 100PP, 30PP70-60D, 50PP50-60D, 50PP50PP, DOMICILIADO. En cualquier otro caso null.
            - "number" es el número de factura del documento; "dynamicsNumber" solo si aparece una referencia
              de Dynamics/Business Central (p. ej. FV0626xx); si no, null.
            - "notes": una observación breve SOLO si hay algo inusual (abono, rectificativa, texto ilegible); si no, null.
            - Si un dato no aparece en el documento, devuélvelo como null. No inventes valores.
            """;
    }

    private static readonly string[] NullableStringType = ["string", "null"];
    private static readonly string[] NullableNumberType = ["number", "null"];
    private static readonly string[] InvoiceTypeValues = ["Emitida", "Recibida"];
    private static readonly string[] RequiredFields =
    [
        "type", "number", "dynamicsNumber", "invoiceDate", "dueDate", "taxBase",
        "vat", "total", "paymentTerms", "counterpartyName", "bank", "notes",
    ];

    private static JsonOutputFormat BuildOutputFormat()
    {
        static JsonElement El(object o) => JsonSerializer.SerializeToElement(o);
        var nullableString = new { type = NullableStringType };
        var nullableNumber = new { type = NullableNumberType };

        return new JsonOutputFormat
        {
            Schema = new Dictionary<string, JsonElement>
            {
                ["type"] = El("object"),
                ["properties"] = El(new Dictionary<string, object>
                {
                    ["type"] = new
                    {
                        anyOf = new object[]
                        {
                            new { type = "string", @enum = InvoiceTypeValues },
                            new { type = "null" },
                        },
                    },
                    ["number"] = nullableString,
                    ["dynamicsNumber"] = nullableString,
                    ["invoiceDate"] = nullableString,
                    ["dueDate"] = nullableString,
                    ["taxBase"] = nullableNumber,
                    ["vat"] = nullableNumber,
                    ["total"] = nullableNumber,
                    ["paymentTerms"] = nullableString,
                    ["counterpartyName"] = nullableString,
                    ["bank"] = nullableString,
                    ["notes"] = nullableString,
                }),
                ["required"] = El(RequiredFields),
                ["additionalProperties"] = El(false),
            },
        };
    }

    private static InvoiceExtractionDto ParseProposal(string json, string documentPath)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        static string? Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null;

        static decimal? Num(JsonElement e, string name) =>
            e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number ? p.GetDecimal() : null;

        static DateTime? Date(JsonElement e, string name)
        {
            string? raw = Str(e, name);
            return DateTime.TryParse(raw, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var d)
                ? DateTime.SpecifyKind(d.Date, DateTimeKind.Unspecified)
                : null;
        }

        InvoiceType? type = Str(root, "type") switch
        {
            "Emitida" => InvoiceType.Emitida,
            "Recibida" => InvoiceType.Recibida,
            _ => null,
        };

        // Only propose payment terms the domain grammar actually accepts — anything else would be
        // rejected on save anyway.
        string? terms = Str(root, "paymentTerms");
        if (!Domain.PaymentTerms.TryParse(terms, out _))
        {
            terms = null;
        }

        return new InvoiceExtractionDto(
            type,
            Str(root, "number"),
            Str(root, "dynamicsNumber"),
            Date(root, "invoiceDate"),
            Date(root, "dueDate"),
            Num(root, "taxBase"),
            Num(root, "vat"),
            Num(root, "total"),
            terms,
            Str(root, "counterpartyName"),
            MatchedClientId: null,
            MatchedSupplierId: null,
            Str(root, "bank"),
            Str(root, "notes"),
            documentPath);
    }

    private async Task<InvoiceExtractionDto> ResolveCounterpartyAsync(
        InvoiceExtractionDto proposal, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(proposal.CounterpartyName))
        {
            return proposal;
        }

        string needle = Normalize(proposal.CounterpartyName);
        if (needle.Length == 0)
        {
            return proposal;
        }

        if (proposal.Type is null or InvoiceType.Emitida)
        {
            var clients = await dbContext.Clients.AsNoTracking()
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var client = clients.FirstOrDefault(c => Matches(Normalize(c.Name), needle));
            if (client is not null)
            {
                return proposal with { Type = proposal.Type ?? InvoiceType.Emitida, MatchedClientId = client.Id };
            }
        }

        if (proposal.Type is null or InvoiceType.Recibida)
        {
            var suppliers = await dbContext.Suppliers.AsNoTracking()
                .Select(s => new { s.Id, s.Name })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
            var supplier = suppliers.FirstOrDefault(s => Matches(Normalize(s.Name), needle));
            if (supplier is not null)
            {
                return proposal with { Type = proposal.Type ?? InvoiceType.Recibida, MatchedSupplierId = supplier.Id };
            }
        }

        return proposal;
    }

    /// <summary>Uppercase, corporate suffixes stripped — the same normalization the data load used.</summary>
    internal static string Normalize(string raw)
    {
        string x = raw.ToUpperInvariant();
        x = SuffixRegex().Replace(x, " ");
        x = NonAlnumRegex().Replace(x, " ");
        return WhitespaceRegex().Replace(x, " ").Trim();
    }

    internal static bool Matches(string candidate, string needle) =>
        candidate.Length > 0 &&
        (candidate.Equals(needle, StringComparison.Ordinal)
            || candidate.Contains(needle, StringComparison.Ordinal)
            || needle.Contains(candidate, StringComparison.Ordinal));

    [System.Text.RegularExpressions.GeneratedRegex(@"\b(S\.?R\.?L\.?|S\.?P\.?A\.?|S\.?L\.?U?\.?|S\.?A\.?U?\.?|LDA|SOCIEDAD|CAPITAL)\b")]
    private static partial System.Text.RegularExpressions.Regex SuffixRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"[^A-Z0-9 ]")]
    private static partial System.Text.RegularExpressions.Regex NonAlnumRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"\s+")]
    private static partial System.Text.RegularExpressions.Regex WhitespaceRegex();

    [LoggerMessage(Level = LogLevel.Information, Message = "AI invoice extraction for {FileName} returned {JsonLength} chars")]
    private static partial void LogExtractionResult(ILogger logger, string fileName, int jsonLength);

    [LoggerMessage(Level = LogLevel.Error, Message = "AI invoice extraction model call failed")]
    private static partial void LogModelCallFailed(ILogger logger, Exception ex);
}
