using FSH.Framework.Core.Exceptions;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoicePdf;

public sealed partial class GetCashflowInvoicePdfQueryHandler(
    CashflowDbContext dbContext,
    ICashflowInvoicePdfRenderer renderer,
    IStorageService storage,
    IHttpClientFactory httpClientFactory,
    ILogger<GetCashflowInvoicePdfQueryHandler> logger)
    : IQueryHandler<GetCashflowInvoicePdfQuery, InvoicePdfDto>
{
    public async ValueTask<InvoicePdfDto> Handle(GetCashflowInvoicePdfQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invoice = await dbContext.Invoices.AsNoTracking()
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == query.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {query.InvoiceId} not found.");

        var client = invoice.ClientId is null
            ? null
            : await dbContext.Clients.AsNoTracking()
                .Where(c => c.Id == invoice.ClientId)
                .Select(c => new { c.Name, c.LegalName, c.TaxId, c.Address })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        // Recibidas: la contraparte es el proveedor — se muestra en el bloque "Cliente" del PDF
        // con su nombre (el PDF de Recibida es un archivo interno, no un documento a enviar).
        string? counterpartyName = client?.LegalName ?? client?.Name;
        if (counterpartyName is null && invoice.SupplierId is not null)
        {
            counterpartyName = await dbContext.Suppliers.AsNoTracking()
                .Where(s => s.Id == invoice.SupplierId).Select(s => s.Name)
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        var company = invoice.CompanyId is null
            ? null
            : await dbContext.Companies.AsNoTracking()
                .Where(c => c.Id == invoice.CompanyId)
                .Select(c => new { c.Name, c.LegalName, c.Nif, c.Address, c.PostalCode, c.City, c.Phone, c.Email, c.LogoPath })
                .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        byte[]? logo = company?.LogoPath is null
            ? null
            : await TryLoadLogoAsync(company.LogoPath, cancellationToken).ConfigureAwait(false);

        var record = await dbContext.InvoiceVerifactuRecords.AsNoTracking()
            .FirstOrDefaultAsync(r => r.InvoiceId == invoice.Id, cancellationToken)
            .ConfigureAwait(false);

        string? addressLine = company is null
            ? null
            : string.Join(" · ", new[] { company.Address, string.Join(" ", new[] { company.PostalCode, company.City }.Where(s => !string.IsNullOrWhiteSpace(s))) }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
        string? contactLine = company is null
            ? null
            : string.Join(" · ", new[] { company.Phone, company.Email }.Where(s => !string.IsNullOrWhiteSpace(s)));

        byte[] content = renderer.Render(new InvoicePdfData(
            invoice.Number ?? "(sin número)",
            invoice.InvoiceDate,
            invoice.DueDate,
            counterpartyName,
            client?.TaxId,
            client?.Address,
            company?.LegalName ?? company?.Name,
            company?.Nif,
            string.IsNullOrWhiteSpace(addressLine) ? null : addressLine,
            string.IsNullOrWhiteSpace(contactLine) ? null : contactLine,
            logo,
            invoice.Items.OrderBy(x => x.Position)
                .Select(x => new InvoicePdfItem(x.Description, x.Quantity, x.UnitPrice, x.Amount))
                .ToList(),
            invoice.TaxBase,
            invoice.Vat,
            invoice.Total,
            invoice.PaymentTerms?.Code,
            invoice.Notes,
            record?.QrPayload,
            record?.Hash));

        return new InvoicePdfDto(content, $"factura-{invoice.Number ?? "sin-numero"}.pdf");
    }

    /// <summary>Best-effort logo fetch: a broken logo must never break the invoice PDF. Upload
    /// persists the public URL on S3-backed storage; bare keys go through the storage service.</summary>
    private async Task<byte[]?> TryLoadLogoAsync(string logoPath, CancellationToken cancellationToken)
    {
        try
        {
            if (logoPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                using var http = httpClientFactory.CreateClient();
                http.Timeout = TimeSpan.FromSeconds(10);
                return await http.GetByteArrayAsync(new Uri(logoPath), cancellationToken).ConfigureAwait(false);
            }

            var download = await storage.DownloadAsync(logoPath, cancellationToken).ConfigureAwait(false);
            if (download is null)
            {
                return null;
            }
            await using var stream = download.Stream;
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
            return ms.ToArray();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogLogoLoadFailed(logger, logoPath, ex);
            return null;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not load company logo {Path}; rendering the PDF without it")]
    private static partial void LogLogoLoadFailed(ILogger logger, string path, Exception ex);
}
