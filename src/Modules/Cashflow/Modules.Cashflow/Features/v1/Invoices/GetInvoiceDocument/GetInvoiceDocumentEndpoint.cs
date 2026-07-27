using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Storage.Services;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GetInvoiceDocument;

public static class GetInvoiceDocumentEndpoint
{
    /// <summary>Returns a short-lived presigned URL for the invoice's attached document so the
    /// browser can open it directly (no auth header needed on the storage host).</summary>
    internal static RouteHandlerBuilder MapGetInvoiceDocumentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/invoices/{id:guid}/document",
                async (Guid id, CashflowDbContext dbContext, IStorageService storage, CancellationToken ct) =>
                {
                    var invoice = await dbContext.Invoices.AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Id == id, ct)
                        ?? throw new NotFoundException($"Invoice {id} not found.");

                    if (string.IsNullOrWhiteSpace(invoice.DocumentPath))
                    {
                        throw new NotFoundException("La factura no tiene documento adjunto.");
                    }

                    // UploadAsync persists the PUBLIC URL (bucket policy grants read) — return it
                    // as-is. A bare storage key (e.g. future local-storage rows) gets presigned.
                    if (invoice.DocumentPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        return Results.Ok(new { url = invoice.DocumentPath });
                    }

                    var url = await storage.GenerateDownloadUrlAsync(
                        invoice.DocumentPath, TimeSpan.FromMinutes(5), cancellationToken: ct);
                    return Results.Ok(new { url = url.ToString() });
                })
            .WithName("GetInvoiceDocumentUrl")
            .WithSummary("Get a presigned URL for the invoice's attached document")
            .RequirePermission(CashflowPermissions.Facturacion.View);
    }
}
