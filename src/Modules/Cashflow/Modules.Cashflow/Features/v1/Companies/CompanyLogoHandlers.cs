using System.Net;
using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Storage;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.v1.Companies;
using FSH.Modules.Cashflow.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StorageFileType = FSH.Framework.Storage.FileType;

namespace FSH.Modules.Cashflow.Features.v1.Companies;

public sealed partial class SetCompanyLogoCommandHandler(
    CashflowDbContext dbContext,
    IStorageService storage,
    ILogger<SetCompanyLogoCommandHandler> logger)
    : ICommandHandler<SetCompanyLogoCommand, string>
{
    // Un logo va incrustado en cada PDF — mantenerlo pequeño.
    private const int MaxLogoBytes = 1024 * 1024;

    public async ValueTask<string> Handle(SetCompanyLogoCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.Content.Length > MaxLogoBytes)
        {
            throw new CustomException("El logo no puede superar 1 MB.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var company = await dbContext.Companies
            .FirstOrDefaultAsync(c => c.Id == command.CompanyId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Company {command.CompanyId} not found.");

        string logoPath = await storage.UploadAsync<Domain.Company>(
            new FileUploadRequest
            {
                FileName = command.FileName,
                ContentType = command.ContentType,
                Data = [.. command.Content],
            },
            StorageFileType.Image,
            cancellationToken).ConfigureAwait(false);

        // Replacing? Best-effort cleanup — a failure must not lose the new logo.
        string? previous = company.LogoPath;
        company.SetLogo(logoPath);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(previous))
        {
            try
            {
                await storage.RemoveAsync(previous, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                LogOldLogoCleanupFailed(logger, previous, ex);
            }
        }

        return logoPath;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Could not remove replaced company logo {Path}")]
    private static partial void LogOldLogoCleanupFailed(ILogger logger, string path, Exception ex);
}

public sealed class SetCompanyLogoCommandValidator : AbstractValidator<SetCompanyLogoCommand>
{
    public SetCompanyLogoCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(128)
            .Must(ct => ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("El logo debe ser una imagen (PNG/JPG).");
    }
}
