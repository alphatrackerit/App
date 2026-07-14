using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Notes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Notes.DeleteNote;

public static class DeleteNoteEndpoint
{
    internal static RouteHandlerBuilder MapDeleteNoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapDelete("/notes/{id:guid}",
                async (Guid id, IMediator mediator, CancellationToken ct) =>
                {
                    await mediator.Send(new DeleteNoteCommand(id), ct);
                    return Results.NoContent();
                })
            .WithName("DeleteNote")
            .WithSummary("Delete a note")
            .RequirePermission(CashflowPermissions.Notes.Delete);
    }
}
