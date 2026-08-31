using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthTracker.Modules.Water.Infrastructure;

public sealed class WaterDbContextFactory : IDesignTimeDbContextFactory<WaterDbContext>
{
    public WaterDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<WaterDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres",
                npg => npg.MigrationsHistoryTable("__ef_migrations", WaterDbContext.Schema))
            .Options;

        return new WaterDbContext(options);
    }
}
