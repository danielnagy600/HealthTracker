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

public static class WaterModule
{
    public static IServiceCollection AddWaterModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton<IClock, SystemClock>();

        services.AddDbContext<WaterDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", WaterDbContext.Schema)));

        services.AddScoped<IWaterRepository, WaterRepository>();
        services.AddScoped<IWaterService, WaterService>();

        return services;
    }

    public static async Task MigrateWaterModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaterDbContext>();
        await db.Database.MigrateAsync();
    }

    public static IEndpointRouteBuilder MapWaterModule(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/water")
            .RequireAuthorization();

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
