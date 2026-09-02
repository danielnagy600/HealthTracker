using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Infrastructure;
using HealthTracker.SharedKernel.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HealthTracker.Modules.Schedule;

public static class ScheduleModule
{
    public static IServiceCollection AddScheduleModule(this IServiceCollection services, string connectionString)
    {
        services.TryAddSingleton<IClock, SystemClock>();

        services.AddDbContext<ScheduleDbContext>(options =>
            options.UseNpgsql(connectionString, npg =>
                npg.MigrationsHistoryTable("__ef_migrations", ScheduleDbContext.Schema)));

        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IScheduleService, ScheduleService>();

        return services;
    }

    public static async Task MigrateScheduleModuleAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ScheduleDbContext>();
        await db.Database.MigrateAsync();
    }

    // A HTTP-végpontok most a Controllers/ScheduleController.cs-ben élnek
    // (attribútum-routing), nem itt – ez a modul innentől csak a DI-t és a
    // migrációt adja a hostnak, a Program.cs egy `app.MapControllers()`
    // hívással köti be az összes vezérlőt.
}
