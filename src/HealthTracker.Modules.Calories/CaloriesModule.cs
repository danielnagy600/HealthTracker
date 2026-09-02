using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Infrastructure;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthTracker.Modules.Calories;

public static class CaloriesModule
{
    public static IServiceCollection AddCaloriesModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton<IClock, SystemClock>();

        services.AddDbContext<CalorieDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", CalorieDbContext.Schema)));

        services.AddScoped<IFoodEntryRepository, FoodEntryRepository>();
        services.AddScoped<ICalorieService, CalorieService>();

        return services;
    }

    public static async Task MigrateCaloriesModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CalorieDbContext>();
        await db.Database.MigrateAsync();
    }

    // A HTTP-végpontok most a Controllers/CaloriesController.cs-ben élnek
    // (attribútum-routing), nem itt – ez a modul innentől csak a DI-t és a
    // migrációt adja a hostnak, a Program.cs egy `app.MapControllers()`
    // hívással köti be az összes vezérlőt.
}
