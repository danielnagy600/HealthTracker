using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthTracker.Modules.Schedule.Infrastructure;

/// <summary>
/// Csak tervezési időben (design-time) használt gyár, amit a "dotnet ef migrations"
/// parancs hív. Így a migrációk generálásához nem kell felhúzni a teljes API-hostot.
/// A kapcsolati sztringnek itt csak érvényesnek kell lennie – az adatbázisnak nem
/// kell futnia a migráció létrehozásához.
/// </summary>
public sealed class ScheduleDbContextFactory : IDesignTimeDbContextFactory<ScheduleDbContext>
{
    public ScheduleDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres",
                npg => npg.MigrationsHistoryTable("__ef_migrations", ScheduleDbContext.Schema))
            .Options;

        return new ScheduleDbContext(options);
    }
}
