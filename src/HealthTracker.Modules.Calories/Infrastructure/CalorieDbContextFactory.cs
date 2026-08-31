using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthTracker.Modules.Calories.Infrastructure;

public sealed class CalorieDbContextFactory : IDesignTimeDbContextFactory<CalorieDbContext>
{
    public CalorieDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CalorieDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres",
                npg => npg.MigrationsHistoryTable("__ef_migrations", CalorieDbContext.Schema))
            .Options;

        return new CalorieDbContext(options);
    }
}
