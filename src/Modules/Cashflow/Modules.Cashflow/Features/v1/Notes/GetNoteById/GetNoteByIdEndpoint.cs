using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Notes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Notes.GetNoteById;

public static class GetNoteByIdEndpoint
{
    internal static RouteHandlerBuilder MapGetNoteByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/notes/{id:guid}",
                (Guid id, IMediator mediator, CancellationToken ct) =>
                    mediator.Send(new GetNoteByIdQuery(id), ct))
            .WithName("GetNoteById")
            .WithSummary("Get a note by id")
            .RequirePermission(CashflowPermissions.Notes.View);
    }
}
