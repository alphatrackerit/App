using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Proformas.GetProformaDocument;

public static class GetProformaDocumentEndpoint
{
    /// <summary>Returns a short-lived presigned URL for the proforma's attached document so the
    /// browser can open it directly (no auth header needed on the storage host).</summary>
    internal static RouteHandlerBuilder MapGetProformaDocumentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/proformas/{id:guid}/document",
                async (Guid id, CashflowDbContext dbContext, IStorageService storage, CancellationToken ct) =>
                {
                    var proforma = await dbContext.Proformas.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == id, ct)
                        ?? throw new NotFoundException($"Proforma {id} not found.");

                    if (string.IsNullOrWhiteSpace(proforma.DocumentPath))
                    {
                        throw new NotFoundException("La proforma no tiene documento adjunto.");
                    }

                    // UploadAsync persists the PUBLIC URL (bucket policy grants read) — return it
                    // as-is. A bare storage key gets presigned.
                    if (proforma.DocumentPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Ok(new { url = proforma.DocumentPath });
                    }

                    var url = await storage.GenerateDownloadUrlAsync(
                        proforma.DocumentPath, TimeSpan.FromMinutes(5), cancellationToken: ct);
                    return Results.Ok(new { url = url.ToString() });
                })
            .WithName("GetProformaDocumentUrl")
            .WithSummary("Get a presigned URL for the proforma's attached document")
            .RequirePermission(CashflowPermissions.Proformas.View);
    }
}
