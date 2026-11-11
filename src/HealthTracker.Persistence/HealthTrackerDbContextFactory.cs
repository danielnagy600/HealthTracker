using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthTracker.Persistence;

public sealed class HealthTrackerDbContextFactory : IDesignTimeDbContextFactory<HealthTrackerDbContext>
{
    public HealthTrackerDbContext CreateDbContext(string[] args)
    {
        // A `dotnet ef` design-time futás nem megy át az alkalmazás normál
        // konfigurációs láncán, ezért itt külön olvassuk ki a ConnectionStrings__Postgres
        // env változót - enélkül ez a factory mindig localhost-ra migrálna, függetlenül
        // attól, milyen célkörnyezetnek szánjuk a migrációt.
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<HealthTrackerDbContext>()
            .UseNpgsql(connectionString, PersistenceServiceRegistration.ConfigureNpgsql)
            .Options;

        return new HealthTrackerDbContext(options);
    }
}
