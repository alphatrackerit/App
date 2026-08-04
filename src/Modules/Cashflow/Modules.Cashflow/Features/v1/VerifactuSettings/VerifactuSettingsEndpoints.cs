using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Verifactu;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.VerifactuSettings;

public static class GetVerifactuSettingsEndpoint
{
    internal static RouteHandlerBuilder MapGetVerifactuSettingsEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/verifactu/settings/{companyId:guid}",
                (Guid companyId, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetVerifactuSettingsQuery(companyId), ct))
            .WithName("GetVerifactuSettings")
            .WithSummary("Get a company's VeriFactu settings (never returns the certificate)")
            .RequirePermission(CashflowPermissions.Facturacion.ManageVerifactuSettings);
}

public static class UpsertVerifactuSettingsEndpoint
{
    internal static RouteHandlerBuilder MapUpsertVerifactuSettingsEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPut("/verifactu/settings",
                async (UpsertVerifactuSettingsCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("UpsertVerifactuSettings")
            .WithSummary("Create or update a company's VeriFactu settings")
            .RequirePermission(CashflowPermissions.Facturacion.ManageVerifactuSettings);
}

public static class SetVerifactuCertificateEndpoint
{
    internal static RouteHandlerBuilder MapSetVerifactuCertificateEndpoint(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/verifactu/settings/{companyId:guid}/certificate",
                async (Guid companyId, IFormFile file, [Microsoft.AspNetCore.Mvc.FromForm] string password, IMediator mediator, CancellationToken ct) =>
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    await mediator.Send(new SetVerifactuCertificateCommand(companyId, ms.ToArray(), password), ct);
                    return Results.NoContent();
                })
            .WithName("SetVerifactuCertificate")
            .WithSummary("Upload the company's client certificate (PFX + password), encrypted at rest")
            .RequirePermission(CashflowPermissions.Facturacion.ManageVerifactuSettings)
            .DisableAntiforgery();
}
