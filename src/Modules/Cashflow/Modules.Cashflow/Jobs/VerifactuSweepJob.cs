using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Jobs.Services;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FSH.Modules.Cashflow.Jobs;

/// <summary>
/// Hourly recurring sweep: for every tenant, finds companies with VeriFactu enabled and records
/// still <c>PendienteEnvio</c>/<c>ErrorTecnico</c>, and enqueues one <see cref="SubmitVerifactuRecordsJob"/>
/// per company. Covers both transient AEAT failures and the "certificate was just uploaded and
/// locally-issued invoices are waiting" scenario — no code change needed when a company goes live.
/// </summary>
public sealed partial class VerifactuSweepJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore;
    private readonly IJobService _jobService;
    private readonly ILogger<VerifactuSweepJob> _logger;

    public VerifactuSweepJob(
        IServiceScopeFactory scopeFactory,
        IMultiTenantStore<AppTenantInfo> tenantStore,
        IJobService jobService,
        ILogger<VerifactuSweepJob> logger)
    {
        _scopeFactory = scopeFactory;
        _tenantStore = tenantStore;
        _jobService = jobService;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var tenants = await _tenantStore.GetAllAsync().ConfigureAwait(false);
        foreach (var tenant in tenants.Where(t => t.IsActive))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                scope.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>()
                    .MultiTenantContext = new MultiTenantContext<AppTenantInfo>(tenant);
                var dbContext = scope.ServiceProvider.GetRequiredService<CashflowDbContext>();

                var companyIds = await (
                    from settings in dbContext.VerifactuSettings
                    where settings.Enabled && settings.EncryptedCertificate != null
                    join invoice in dbContext.Invoices on settings.CompanyId equals invoice.CompanyId
                    join record in dbContext.InvoiceVerifactuRecords on invoice.Id equals record.InvoiceId
                    where record.Status == VerifactuStatus.PendienteEnvio || record.Status == VerifactuStatus.ErrorTecnico
                    select settings.CompanyId)
                    .Distinct()
                    .ToListAsync(cancellationToken).ConfigureAwait(false);

                foreach (var companyId in companyIds)
                {
                    string tenantId = tenant.Id!;
                    _jobService.Enqueue<SubmitVerifactuRecordsJob>(
                        j => j.SubmitPendingAsync(tenantId, companyId, CancellationToken.None));
                }

                if (companyIds.Count > 0)
                {
                    LogEnqueued(_logger, companyIds.Count, tenant.Id!);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Keep sweeping the remaining tenants — one broken tenant must not stall the rest.
                LogTenantSweepFailed(_logger, tenant.Id!, ex);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[VeriFactu] sweep enqueued {Count} company submission(s) for tenant {TenantId}")]
    private static partial void LogEnqueued(ILogger logger, int count, string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[VeriFactu] sweep failed for tenant {TenantId}; continuing")]
    private static partial void LogTenantSweepFailed(ILogger logger, string tenantId, Exception ex);
}
