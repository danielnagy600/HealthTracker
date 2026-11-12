using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthTracker.Persistence;

public sealed class HealthTrackerDbContextFactory : IDesignTimeDbContextFactory<HealthTrackerDbContext>
{
    public HealthTrackerDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<HealthTrackerDbContext>()
            .UseNpgsql(connectionString, PersistenceServiceRegistration.ConfigureNpgsql)
            .Options;

        return new HealthTrackerDbContext(options);
    }
}
