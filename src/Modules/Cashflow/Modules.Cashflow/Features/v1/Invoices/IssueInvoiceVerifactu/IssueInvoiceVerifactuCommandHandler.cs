using System.Net;
using Finbuckle.MultiTenant.Abstractions;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Jobs.Services;
using FSH.Framework.Shared.Multitenancy;
using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Verifactu;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using FSH.Modules.Cashflow.Jobs;
using FSH.Modules.Cashflow.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.IssueInvoiceVerifactu;

public sealed class IssueInvoiceVerifactuCommandHandler(
    CashflowDbContext dbContext,
    IVerifactuChainService chainService,
    IJobService jobService,
    IMultiTenantContextAccessor<AppTenantInfo> tenantAccessor)
    : ICommandHandler<IssueInvoiceVerifactuCommand, InvoiceVerifactuRecordDto>
{
    public async ValueTask<InvoiceVerifactuRecordDto> Handle(IssueInvoiceVerifactuCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

        if (invoice.Type != InvoiceType.Emitida)
        {
            throw new CustomException(
                "Solo las facturas emitidas pasan por VERI*FACTU (la obligación es de quien emite).", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        if (invoice.VerifactuStatus != VerifactuStatus.NoAplica)
        {
            throw new CustomException(
                "La factura ya está registrada en VERI*FACTU.", Array.Empty<string>(), HttpStatusCode.Conflict);
        }

        if (invoice.CompanyId is not { } companyId)
        {
            throw new CustomException(
                "La factura no tiene empresa asignada — VERI*FACTU registra por NIF de empresa.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        if (invoice.InvoiceDate is not { } invoiceDate)
        {
            throw new CustomException(
                "La factura no tiene fecha de expedición (FechaFactura), obligatoria en el registro AEAT.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var company = await dbContext.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Company {companyId} not found.");

        if (string.IsNullOrWhiteSpace(company.Nif))
        {
            throw new CustomException(
                $"La empresa \"{company.Name}\" no tiene NIF configurado — obligatorio para VERI*FACTU.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        // Serialise chaining per company: two invoices of the same Company must never compute the
        // same PreviousHash in parallel. pg_advisory_xact_lock holds until the transaction ends.
        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        long lockKey = BitConverter.ToInt64(companyId.ToByteArray(), 0);
        await dbContext.Database
            .ExecuteSqlAsync($"SELECT pg_advisory_xact_lock({lockKey})", cancellationToken)
            .ConfigureAwait(false);

        // Settings are read AFTER the lock so LastChainHash is the true chain head.
        var settings = await dbContext.VerifactuSettings
            .FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new CustomException(
                $"VERI*FACTU no está configurado para la empresa \"{company.Name}\" — crea sus ajustes primero.", Array.Empty<string>(), HttpStatusCode.BadRequest);

        var generatedAt = DateTimeOffset.Now;
        var result = chainService.BuildRecord(
            new VerifactuChainInput(
                company.Nif, invoice.Number, invoiceDate,
                invoice.Vat ?? 0m, invoice.Total,
                settings.LastChainHash, generatedAt),
            settings.Environment);

        var record = InvoiceVerifactuRecord.Create(
            invoice.Id, settings.LastChainHash, result.Hash, generatedAt, result.QrPayload);
        dbContext.InvoiceVerifactuRecords.Add(record);

        invoice.MarkVerifactuIssued();
        settings.AdvanceChain(result.Hash, generatedAt);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        // Enabled + certificate → schedule the submission with the 60 s flow-control delay the
        // AEAT mandates between envíos; rapid consecutive issues coalesce into one batch send.
        if (settings.Enabled && settings.HasCertificate
            && tenantAccessor.MultiTenantContext?.TenantInfo?.Id is { } tenantId)
        {
            jobService.Schedule<SubmitVerifactuRecordsJob>(
                j => j.SubmitPendingAsync(tenantId, companyId, CancellationToken.None),
                TimeSpan.FromSeconds(60));
        }

        return new InvoiceVerifactuRecordDto(
            record.Id, record.InvoiceId, record.PreviousHash, record.Hash,
            record.GeneratedAt, record.QrPayload, record.Status, record.AeatResponseCode, record.RetryCount);
    }
}
