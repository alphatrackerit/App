using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Storage;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.v1.Proformas;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorageFileType = FSH.Framework.Storage.FileType;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.AttachProformaDocument;

public sealed partial class AttachProformaDocumentCommandHandler(
    CashflowDbContext dbContext,
    IStorageService storage,
    ILogger<AttachProformaDocumentCommandHandler> logger)
    : ICommandHandler<AttachProformaDocumentCommand, string>
{
    public async ValueTask<string> Handle(AttachProformaDocumentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var proforma = await dbContext.Proformas
            .FirstOrDefaultAsync(p => p.Id == command.ProformaId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Proforma {command.ProformaId} not found.");

        bool isPdf = string.Equals(command.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
        string documentPath = await storage.UploadAsync<Domain.Proforma>(
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
        string? previous = proforma.DocumentPath;
        proforma.AttachDocument(documentPath);
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not remove replaced proforma document {Path}")]
    private static partial void LogOldDocumentCleanupFailed(ILogger logger, string path, Exception ex);
}
