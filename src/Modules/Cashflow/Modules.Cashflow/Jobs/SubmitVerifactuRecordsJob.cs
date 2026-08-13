using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Cashflow.Jobs;

/// <summary>
/// Hangfire job that submits a company's pending VERI*FACTU records to the AEAT in one batch
/// (RegFactuSistemaFacturacion admits up to 1000 records per envelope; batching also satisfies the
/// 60-second flow-control window — the enqueuer schedules this job with a 60 s delay so rapid
/// consecutive issues coalesce into a single send). Transient/transport failures throw so Hangfire
/// retries; AEAT business rejections are terminal per record (Rechazada) and never retried.
/// </summary>
public sealed partial class SubmitVerifactuRecordsJob
{
    private const int MaxRecordsPerEnvelope = 1000;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAeatVerifactuClient _client;
    private readonly IVerifactuSecretProtector _protector;
    private readonly ILogger<SubmitVerifactuRecordsJob> _logger;

    public SubmitVerifactuRecordsJob(
        IServiceScopeFactory scopeFactory,
        IAeatVerifactuClient client,
        IVerifactuSecretProtector protector,
        ILogger<SubmitVerifactuRecordsJob> logger)
    {
        _scopeFactory = scopeFactory;
        _client = client;
        _protector = protector;
        _logger = logger;
    }

    // 30s, 2m, 10m, 1h, 6h — after exhaustion the hourly sweep keeps retrying ErrorTecnico records.
    [AutomaticRetry(
        Attempts = 5,
        DelaysInSeconds = new[] { 30, 120, 600, 3600, 21600 },
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task SubmitPendingAsync(string tenantId, Guid companyId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        // Jobs run with no HTTP context — restore the Finbuckle tenant before touching the DbContext.
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<AppTenantInfo>>();
        var tenant = await store.GetAsync(tenantId).ConfigureAwait(false);
        if (tenant is null)
        {
            LogTenantNotFound(_logger, tenantId);
            return;
        }
        scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
            .MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant);

        var dbContext = scope.ServiceProvider.GetRequiredService<CashflowDbContext>();

        var settings = await dbContext.VerifactuSettings
            .FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken)
            .ConfigureAwait(false);
        if (settings is null || !settings.Enabled || !settings.HasCertificate)
        {
            LogSkipped(_logger, companyId, tenantId);
            return;
        }

        var company = await dbContext.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken).ConfigureAwait(false);
        if (company?.Nif is null)
        {
            LogSkipped(_logger, companyId, tenantId);
            return;
        }

        // Pending or previously-failed records of this company's invoices, oldest first (chain order).
        var batch = await (
            from record in dbContext.InvoiceVerifactuRecords
            join invoice in dbContext.Invoices on record.InvoiceId equals invoice.Id
            where invoice.CompanyId == companyId
                && (record.Status == VerifactuStatus.PendienteEnvio || record.Status == VerifactuStatus.ErrorTecnico)
            orderby record.GeneratedAt
            select new { Record = record, Invoice = invoice })
            .Take(MaxRecordsPerEnvelope)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        if (batch.Count == 0)
        {
            return;
        }

        // Resolve previous-record identities (Encadenamiento/RegistroAnterior) and recipient NIFs.
        var previousHashes = batch.Select(b => b.Record.PreviousHash).Where(h => h != null).Cast<string>().ToList();
        var previousByHash = await (
            from record in dbContext.InvoiceVerifactuRecords
            join invoice in dbContext.Invoices on record.InvoiceId equals invoice.Id
            where previousHashes.Contains(record.Hash)
            select new { record.Hash, invoice.Number, invoice.InvoiceDate })
            .ToDictionaryAsync(x => x.Hash, cancellationToken).ConfigureAwait(false);

        var clientIds = batch.Select(b => b.Invoice.ClientId).Where(id => id != null).Cast<Guid>().Distinct().ToList();
        var clients = await dbContext.Clients.AsNoTracking()
            .Where(c => clientIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name, c.TaxId })
            .ToDictionaryAsync(c => c.Id, cancellationToken).ConfigureAwait(false);

        var records = batch.Select(b =>
        {
            var prev = b.Record.PreviousHash is { } ph && previousByHash.TryGetValue(ph, out var p)
                ? new AeatPreviousRecordRef(company.Nif, p.Number!, p.InvoiceDate ?? b.Invoice.InvoiceDate!.Value, ph)
                : null;
            var recipient = b.Invoice.ClientId is { } cid && clients.TryGetValue(cid, out var cl) ? cl : null;
            decimal taxBase = b.Invoice.TaxBase ?? b.Invoice.Total - (b.Invoice.Vat ?? 0m);
            decimal? rate = taxBase > 0 && b.Invoice.Vat is { } vat ? Math.Round(vat / taxBase * 100m, 2) : null;
            // VeriFactu records only exist for Emitidas, which always carry a number (DB check constraint).
            return new AeatAltaRecord(
                b.Invoice.Number!,
                b.Invoice.InvoiceDate!.Value,
                string.IsNullOrWhiteSpace(b.Invoice.Notes) ? "Prestación de servicios / entrega de bienes" : b.Invoice.Notes!,
                recipient?.Name,
                recipient?.TaxId,
                taxBase,
                rate,
                b.Invoice.Vat ?? 0m,
                b.Invoice.Total,
                prev,
                b.Record.GeneratedAt,
                b.Record.Hash);
        }).ToList();

        var submission = new AeatSubmission(
            settings.Environment,
            _protector.Unprotect(settings.EncryptedCertificate!),
            _protector.UnprotectString(settings.EncryptedCertificatePassword!),
            company.LegalName ?? company.Name,
            company.Nif,
            settings.SoftwareName ?? "FSH Starter Cashflow",
            "01",
            settings.SoftwareVersion ?? "1.0",
            settings.InstallationNumber ?? "1",
            records);

        try
        {
            var result = await _client.SubmitAsync(submission, cancellationToken).ConfigureAwait(false);

            foreach (var b in batch)
            {
                var line = result.Records.FirstOrDefault(r => r.InvoiceNumber == b.Invoice.Number);
                var status = line?.Status ?? VerifactuStatus.ErrorTecnico;
                b.Record.MarkSent(result.RequestXml);
                b.Record.MarkResult(status, line?.Code, result.ResponseXml);
                b.Invoice.MarkVerifactuResult(status);
            }
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LogSubmitted(_logger, batch.Count, company.Name, result.GlobalStatus ?? "?");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Transport/TLS/parse failure — mark every record and rethrow so Hangfire retries.
            foreach (var b in batch)
            {
                b.Record.MarkTechnicalError();
                b.Invoice.MarkVerifactuResult(VerifactuStatus.ErrorTecnico);
            }
            await dbContext.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false);
            LogSubmitFailed(_logger, company.Name, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "[VeriFactu] tenant {TenantId} not found; skipping submission")]
    private static partial void LogTenantNotFound(ILogger logger, string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[VeriFactu] company {CompanyId} of tenant {TenantId} skipped (disabled, no certificate or no NIF)")]
    private static partial void LogSkipped(ILogger logger, Guid companyId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "[VeriFactu] submitted {Count} record(s) for {Company}; AEAT estado {Estado}")]
    private static partial void LogSubmitted(ILogger logger, int count, string company, string estado);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[VeriFactu] submission for {Company} failed transiently")]
    private static partial void LogSubmitFailed(ILogger logger, string company, Exception ex);
}
