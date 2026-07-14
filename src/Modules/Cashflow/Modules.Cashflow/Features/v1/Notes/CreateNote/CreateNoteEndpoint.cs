using FSH.Framework.Shared.Identity.Authorization;
using FSH.Framework.Web.Idempotency;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Notes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Notes.CreateNote;

public static class CreateNoteEndpoint
{
    internal static RouteHandlerBuilder MapCreateNoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPost("/notes",
                async (CreateNoteCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command, ct)))
            .WithName("CreateNote")
            .WithSummary("Create a note")
            .RequirePermission(CashflowPermissions.Notes.Create)
            .WithIdempotency();
    }
}
