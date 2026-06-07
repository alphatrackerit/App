using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Projects.Contracts.Authorization;
using FSH.Modules.Projects.Contracts.v1.Notes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Projects.Features.v1.Notes.UpdateNote;

public static class UpdateNoteEndpoint
{
    internal static RouteHandlerBuilder MapUpdateNoteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapPut("/notes/{id:guid}",
                async (Guid id, UpdateNoteCommand command, IMediator mediator, CancellationToken ct) =>
                    Results.Ok(await mediator.Send(command with { NoteId = id }, ct)))
            .WithName("UpdateNote")
            .WithSummary("Update a note")
            .RequirePermission(ProjectsPermissions.Notes.Update);
    }
}
