using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace HealthTracker.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<HealthTrackerDbContext>(options =>
            options.UseNpgsql(connectionString, ConfigureNpgsql));

        services.AddScoped<IWaterRepository, WaterRepository>();
        services.AddScoped<IFoodEntryRepository, FoodEntryRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();

        return services;
    }

    public static async Task MigrateDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HealthTrackerDbContext>();
        await db.Database.MigrateAsync();
    }

    // Közös Npgsql-beállítás a futásidejű és a design-time (dotnet ef) contexthez.
    internal static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npg) =>
        npg.MigrationsHistoryTable(
            HealthTrackerDbContext.MigrationsHistoryTable,
            HealthTrackerDbContext.MigrationsHistorySchema);
}
