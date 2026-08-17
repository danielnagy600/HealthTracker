using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Domain;
using HealthTracker.Modules.Calories.Infrastructure;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthTracker.Modules.Calories;

/// <summary>
/// A Calories modul "belépési pontja". Egy helyen írja le, mit ad hozzá a modul a
/// DI-konténerhez, és milyen HTTP-végpontokat tesz közzé. A host (Api) csak ezt a
/// két metódust hívja – a modul belső felépítését nem kell ismernie.
/// </summary>
public static class CaloriesModule
{
    /// <summary>A modul szolgáltatásainak regisztrálása.</summary>
    public static IServiceCollection AddCaloriesModule(this IServiceCollection services, string connectionString)
    {
        // Több modul is kérheti; a TryAdd biztosítja, hogy csak egyszer regisztrálódjon.
        services.TryAddSingleton<IClock, SystemClock>();

        services.AddDbContext<CalorieDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", CalorieDbContext.Schema)));

        services.AddScoped<IFoodEntryRepository, FoodEntryRepository>();
        services.AddScoped<ICalorieService, CalorieService>();

        return services;
    }

    /// <summary>A modul adatbázis-migrációinak alkalmazása induláskor.</summary>
    public static async Task MigrateCaloriesModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CalorieDbContext>();
        await db.Database.MigrateAsync();
    }

    /// <summary>A modul HTTP-végpontjai. Mind bejelentkezést igényel.</summary>
    public static IEndpointRouteBuilder MapCaloriesModule(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/calories")
            .RequireAuthorization() // csak bejelentkezett felhasználó érheti el
            .WithTags("Calories");

        // Egy nap teljes képe étkezésenkénti bontásban. A ?date= elhagyható.
        group.MapGet("/day", async (DateOnly? date, ICalorieService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetDayAsync(date, ct)));

        group.MapPost("/entries", async (SaveFoodEntryRequest req, ICalorieService svc, CancellationToken ct) =>
        {
            if (Validate(req) is { } error)
                return Results.BadRequest(error);

            var item = await svc.AddAsync(req, ct);
            return Results.Created($"/api/calories/entries/{item.Id}", item);
        });

        group.MapPut("/entries/{id:guid}",
            async (Guid id, SaveFoodEntryRequest req, ICalorieService svc, CancellationToken ct) =>
            {
                if (Validate(req) is { } error)
                    return Results.BadRequest(error);

                var item = await svc.UpdateAsync(id, req, ct);
                return item is null ? Results.NotFound() : Results.Ok(item);
            });

        group.MapDelete("/entries/{id:guid}", async (Guid id, ICalorieService svc, CancellationToken ct) =>
            await svc.DeleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

        group.MapGet("/goal", async (ICalorieService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetGoalAsync(ct)));

        group.MapPut("/goal", async (UpdateGoalRequest req, ICalorieService svc, CancellationToken ct) =>
        {
            if (CalorieGoal.Validate(req.DailyTargetKcal) is { } error)
                return Results.BadRequest(error);

            return Results.Ok(await svc.UpdateGoalAsync(req, ct));
        });

        // A választható étkezések – a felület innen is felépíthetné a listát.
        group.MapGet("/meals", () => Results.Ok(Enum.GetNames<MealType>()));

        return app;
    }

    /// <summary>
    /// A kérés ellenőrzése a domain szabályaival. Így a HTTP-réteg 400-as választ
    /// tud adni kivétel helyett, de a szabály maga egy helyen, az entitásban él.
    /// </summary>
    private static string? Validate(SaveFoodEntryRequest req)
    {
        if (!CalorieService.IsKnownMeal(req.Meal))
            return $"Unknown meal: '{req.Meal}'. Available: {string.Join(", ", Enum.GetNames<MealType>())}.";

        return FoodEntry.Validate(req.Name, req.Calories);
    }
}
