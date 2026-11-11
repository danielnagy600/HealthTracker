using System.Security.Claims;
using HealthTracker.Persistence.Identity;

namespace HealthTracker.Api;

// Az Identity marad minimal API (beépített AddIdentityApiEndpoints/MapIdentityApi,
// a tokenkezelést kézi Controllerben újraírni biztonsági kockázat lenne).
internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapIdentityApi<AppUser>();

        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            id = user.FindFirstValue(ClaimTypes.NameIdentifier),
            email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name
        })).RequireAuthorization();

        return app;
    }
}
