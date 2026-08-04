using System.Net;
using System.Security.Cryptography.X509Certificates;
using FluentValidation;
using FSH.Framework.Core.Exceptions;
using FSH.Modules.Cashflow.Contracts.v1.Verifactu;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.VerifactuSettings;

public sealed class GetVerifactuSettingsQueryHandler(CashflowDbContext dbContext)
    : IQueryHandler<GetVerifactuSettingsQuery, VerifactuSettingsDto>
{
    public async ValueTask<VerifactuSettingsDto> Handle(GetVerifactuSettingsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var settings = await dbContext.VerifactuSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.CompanyId == query.CompanyId, cancellationToken)
            .ConfigureAwait(false);

        // No row yet → defaults, so the UI can render the form without a 404 special-case.
        return settings is null
            ? new VerifactuSettingsDto(query.CompanyId, Contracts.Enums.VerifactuEnvironment.Pruebas, false, null, null, null, false, null, null)
            : new VerifactuSettingsDto(
                settings.CompanyId, settings.Environment, settings.Enabled,
                settings.SoftwareName, settings.SoftwareVersion, settings.InstallationNumber,
                settings.HasCertificate, settings.LastChainHash, settings.LastChainAt);
    }
}

public sealed class UpsertVerifactuSettingsCommandHandler(CashflowDbContext dbContext)
    : ICommandHandler<UpsertVerifactuSettingsCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpsertVerifactuSettingsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        bool companyExists = await dbContext.Companies
            .AnyAsync(c => c.Id == command.CompanyId, cancellationToken).ConfigureAwait(false);
        if (!companyExists)
        {
            throw new NotFoundException($"Company {command.CompanyId} not found.");
        }

        var settings = await dbContext.VerifactuSettings
            .FirstOrDefaultAsync(s => s.CompanyId == command.CompanyId, cancellationToken)
            .ConfigureAwait(false);
        if (settings is null)
        {
            settings = Domain.VerifactuSettings.Create(command.CompanyId);
            dbContext.VerifactuSettings.Add(settings);
        }

        if (command.Enabled && !settings.HasCertificate)
        {
            throw new CustomException(
                "No se puede activar el envío a la AEAT sin certificado — sube primero el certificado de la empresa.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        settings.Update(command.Environment, command.Enabled, command.SoftwareName, command.SoftwareVersion, command.InstallationNumber);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return settings.Id;
    }
}

public sealed class UpsertVerifactuSettingsCommandValidator : AbstractValidator<UpsertVerifactuSettingsCommand>
{
    public UpsertVerifactuSettingsCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Environment).IsInEnum();
        RuleFor(x => x.SoftwareName).MaximumLength(128);
        RuleFor(x => x.SoftwareVersion).MaximumLength(64);
        RuleFor(x => x.InstallationNumber).MaximumLength(64);
    }
}

public sealed class SetVerifactuCertificateCommandHandler(
    CashflowDbContext dbContext,
    IVerifactuSecretProtector protector)
    : ICommandHandler<SetVerifactuCertificateCommand, Unit>
{
    public async ValueTask<Unit> Handle(SetVerifactuCertificateCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        bool companyExists = await dbContext.Companies
            .AnyAsync(c => c.Id == command.CompanyId, cancellationToken).ConfigureAwait(false);
        if (!companyExists)
        {
            throw new NotFoundException($"Company {command.CompanyId} not found.");
        }

        // Fail fast on a bad PFX/password — better a clear 400 now than a TLS failure at send time.
        try
        {
            using var certificate = X509CertificateLoader.LoadPkcs12(command.PfxContent, command.Password);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new CustomException(
                "El certificado no se pudo abrir con esa contraseña — comprueba el fichero PFX/P12 y la clave.", Array.Empty<string>(), HttpStatusCode.BadRequest);
        }

        var settings = await dbContext.VerifactuSettings
            .FirstOrDefaultAsync(s => s.CompanyId == command.CompanyId, cancellationToken)
            .ConfigureAwait(false);
        if (settings is null)
        {
            settings = Domain.VerifactuSettings.Create(command.CompanyId);
            dbContext.VerifactuSettings.Add(settings);
        }

        settings.SetCertificate(
            protector.Protect(command.PfxContent),
            protector.ProtectString(command.Password));

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class SetVerifactuCertificateCommandValidator : AbstractValidator<SetVerifactuCertificateCommand>
{
    public SetVerifactuCertificateCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PfxContent).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}
