using HealthTracker.Modules.Water.Application;
using HealthTracker.Modules.Water.Infrastructure;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthTracker.Modules.Water;

/// <summary>
/// A Water modul "belépési pontja". Egy helyen írja le, mit ad hozzá a modul a
/// DI-konténerhez, és milyen HTTP-végpontokat tesz közzé. A host (Api) csak ezt a
/// két metódust hívja – a modul belső felépítését nem kell ismernie.
/// </summary>
public static class WaterModule
{
    /// <summary>A modul szolgáltatásainak regisztrálása.</summary>
    public static IServiceCollection AddWaterModule(this IServiceCollection services, string connectionString)
    {
        // Több modul is kérheti; a TryAdd biztosítja, hogy csak egyszer regisztrálódjon.
        services.TryAddSingleton<IClock, SystemClock>();

        services.AddDbContext<WaterDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", WaterDbContext.Schema)));

        services.AddScoped<IWaterRepository, WaterRepository>();
        services.AddScoped<IWaterService, WaterService>();

        return services;
    }

    /// <summary>A modul adatbázis-migrációinak alkalmazása induláskor.</summary>
    public static async Task MigrateWaterModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaterDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>A modul HTTP-végpontjai. Mind bejelentkezést igényel.</summary>
    public static IEndpointRouteBuilder MapWaterModule(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/water")
            .RequireAuthorization(); // csak bejelentkezett felhasználó érheti el

        group.MapGet("/summary", async (IWaterService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetTodaySummaryAsync(ct)));

        group.MapGet("/reminder", async (IWaterService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetReminderAsync(ct)));

        group.MapPost("/intake", async (AddIntakeRequest req, IWaterService svc, CancellationToken ct) =>
        {
            if (req.AmountMl <= 0)
                return Results.BadRequest("AmountMl must be positive.");

            var item = await svc.AddIntakeAsync(req, ct);
            return Results.Created($"/api/water/intake/{item.Id}", item);
        });

        group.MapGet("/settings", async (IWaterService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetSettingsAsync(ct)));

        group.MapPut("/settings", async (UpdateSettingsRequest req, IWaterService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateSettingsAsync(req, ct)));

        return app;
    }
}
