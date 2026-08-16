using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Domain;
using HealthTracker.Modules.Schedule.Infrastructure;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthTracker.Modules.Schedule;

/// <summary>
/// A Schedule modul "belépési pontja". Egy helyen írja le, mit ad hozzá a modul a
/// DI-konténerhez, és milyen HTTP-végpontokat tesz közzé. A host (Api) csak ezt a
/// két metódust hívja – a modul belső felépítését nem kell ismernie.
/// </summary>
public static class ScheduleModule
{
    /// <summary>A modul szolgáltatásainak regisztrálása.</summary>
    public static IServiceCollection AddScheduleModule(this IServiceCollection services, string connectionString)
    {
        // Több modul is kérheti; a TryAdd biztosítja, hogy csak egyszer regisztrálódjon.
        services.TryAddSingleton<IClock, SystemClock>();

        services.AddDbContext<ScheduleDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", ScheduleDbContext.Schema)));

        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IScheduleService, ScheduleService>();

        return services;
    }

    /// <summary>A modul adatbázis-migrációinak alkalmazása induláskor.</summary>
    public static async Task MigrateScheduleModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>A modul HTTP-végpontjai. Mind bejelentkezést igényel.</summary>
    public static IEndpointRouteBuilder MapScheduleModule(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/schedule")
            .RequireAuthorization() // csak bejelentkezett felhasználó érheti el
            .WithTags("Schedule");

        // Egy nap teljes képe. A ?date= elhagyható – akkor a mai nap jön.
        group.MapGet("/day", async (DateOnly? date, IScheduleService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetDayAsync(date, ct)));

        group.MapPost("/activities", async (SaveActivityRequest req, IScheduleService svc, CancellationToken ct) =>
        {
            if (Validate(req) is { } error)
                return Results.BadRequest(error);

            var item = await svc.AddAsync(req, ct);
            return Results.Created($"/api/schedule/activities/{item.Id}", item);
        });

        group.MapPut("/activities/{id:guid}",
            async (Guid id, SaveActivityRequest req, IScheduleService svc, CancellationToken ct) =>
            {
                if (Validate(req) is { } error)
                    return Results.BadRequest(error);

                var item = await svc.UpdateAsync(id, req, ct);
                return item is null ? Results.NotFound() : Results.Ok(item);
            });

        group.MapDelete("/activities/{id:guid}", async (Guid id, IScheduleService svc, CancellationToken ct) =>
            await svc.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

        // A választható színek – a felület innen is felépíthetné a palettát.
        group.MapGet("/colors", () => Results.Ok(Enum.GetNames<ActivityColor>()));

        return app;
    }

    /// <summary>
    /// A kérés ellenőrzése a domain szabályaival. Így a HTTP-réteg 400-as választ
    /// tud adni kivétel helyett, de a szabály maga egy helyen, az entitásban él.
    /// </summary>
    private static string? Validate(SaveActivityRequest req)
    {
        if (!ScheduleService.IsKnownColor(req.Color))
            return $"Ismeretlen szín: '{req.Color}'. Elérhető: {string.Join(", ", Enum.GetNames<ActivityColor>())}.";

        return Activity.Validate(req.StartTime, req.EndTime, req.Title, req.Note);
    }
}
