using FSH.Framework.Shared.Identity.Authorization;
using FSH.Modules.Cashflow.Contracts.Authorization;
using FSH.Modules.Cashflow.Contracts.v1.Notes;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Cashflow.Features.v1.Notes.SearchNotes;

public static class SearchNotesEndpoint
{
    internal static RouteHandlerBuilder MapSearchNotesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/notes",
                (string? search, int? pageNumber, int? pageSize, string? sortBy, string? sortDir, Guid? projectId,
                 IMediator mediator, CancellationToken ct) =>
                    mediator.Send(
                        new SearchNotesQuery(
                            search,
                            pageNumber ?? 1,
                            pageSize ?? 20,
                            sortBy,
                            sortDir,
                            projectId),
                        ct))
            .WithName("SearchNotes")
            .WithSummary("Search notes (paged, sortable, filter by project)")
            .RequirePermission(CashflowPermissions.Notes.View);
    }
}
