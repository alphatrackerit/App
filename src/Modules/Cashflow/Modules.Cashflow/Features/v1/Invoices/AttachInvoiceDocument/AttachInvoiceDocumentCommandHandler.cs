using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Storage;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorageFileType = FSH.Framework.Storage.FileType;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.AttachInvoiceDocument;

public sealed partial class AttachInvoiceDocumentCommandHandler(
    CashflowDbContext dbContext,
    IStorageService storage,
    ILogger<AttachInvoiceDocumentCommandHandler> logger)
    : ICommandHandler<AttachInvoiceDocumentCommand, string>
{
    public async ValueTask<string> Handle(AttachInvoiceDocumentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Invoice {command.InvoiceId} not found.");

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

        // Replacing? Best-effort cleanup of the previous object — a failure here must not lose
        // the new attachment.
        string? previous = invoice.DocumentPath;
        invoice.AttachDocument(documentPath);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(previous))
        {
            try
            {
                await storage.RemoveAsync(previous, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogOldDocumentCleanupFailed(logger, previous, ex);
            }
        }

        return documentPath;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not remove replaced invoice document {Path}")]
    private static partial void LogOldDocumentCleanupFailed(ILogger logger, string path, Exception ex);
}
