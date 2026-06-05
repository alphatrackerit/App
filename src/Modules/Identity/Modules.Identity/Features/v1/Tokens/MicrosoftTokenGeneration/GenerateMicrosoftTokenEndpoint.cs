using FSH.Modules.Identity.Contracts.DTOs;
using FSH.Modules.Identity.Contracts.v1.Tokens.MicrosoftTokenGeneration;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FSH.Modules.Identity.Features.v1.Tokens.MicrosoftTokenGeneration;

public static class GenerateMicrosoftTokenEndpoint
{
    public static RouteHandlerBuilder MapGenerateMicrosoftTokenEndpoint(this IEndpointRouteBuilder endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        return endpoint.MapPost("/token/microsoft",
            [AllowAnonymous] async Task<Results<Ok<TokenResponse>, UnauthorizedHttpResult, ProblemHttpResult>>
            ([FromBody] GenerateMicrosoftTokenCommand command,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
            {
                var token = await mediator.Send(command, ct);
                return token is null
                    ? TypedResults.Unauthorized()
                    : TypedResults.Ok(token);
            })
            .WithName("IssueMicrosoftJwtTokens")
            .WithSummary("Exchange a Microsoft (Entra ID) ID token for FSH JWT tokens")
            .WithDescription("The dashboard obtains a Microsoft ID token via MSAL and posts it here. The tenant is derived from the verified email domain; only existing, active users are accepted (no auto-provisioning). No 'tenant' header is required.")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
