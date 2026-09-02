using HealthTracker.Modules.Water.Application;
using HealthTracker.Modules.Water.Infrastructure;
using HealthTracker.SharedKernel.Abstractions;
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

    // A HTTP-végpontok most a Controllers/WaterController.cs-ben élnek
    // (attribútum-routing), nem itt – ez a modul innentől csak a DI-t és a
    // migrációt adja a hostnak, a Program.cs egy `app.MapControllers()`
    // hívással köti be az összes vezérlőt.
}
