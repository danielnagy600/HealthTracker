using System.Security.Claims;
using HealthTracker.Modules.Identity.Domain;
using HealthTracker.Modules.Identity.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HealthTracker.Modules.Identity;

/// <summary>
/// Az Identity modul belépési pontja: regisztrálja az ASP.NET Core Identity-t
/// bearer tokenes (API) bejelentkezéssel, és közzéteszi a /api/auth végpontokat
/// (register, login, refresh, stb.), plusz egy /api/auth/me profil-lekérdezést.
/// </summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", AuthDbContext.Schema)));

        services.AddAuthorization();

        // Kész register/login végpontok bearer tokennel (nincs kézi JWT-kódolás).
        services
            .AddIdentityApiEndpoints<AppUser>()
            .AddEntityFrameworkStores<AuthDbContext>();

        return services;
    }

    public static async Task MigrateIdentityModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.MigrateAsync();
    }

    public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // /register, /login, /refresh, /manage/info, stb. – az Identity adja készen.
        group.MapIdentityApi<AppUser>();

        // A bejelentkezett felhasználó alapadatai (profil).
        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            id = user.FindFirstValue(ClaimTypes.NameIdentifier),
            email = user.FindFirstValue(ClaimTypes.Email) ?? user.Identity?.Name
        })).RequireAuthorization();

        return app;
    }
}
