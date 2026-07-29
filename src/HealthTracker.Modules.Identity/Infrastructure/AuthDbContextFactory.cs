using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HealthTracker.Modules.Identity.Infrastructure;

/// <summary>
/// Tervezési idejű gyár a "dotnet ef migrations" parancsnak (lásd a Water modul
/// hasonló gyárát). Az adatbázisnak nem kell futnia a migráció létrehozásához.
/// </summary>
public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=healthtracker;Username=postgres;Password=postgres",
                npg => npg.MigrationsHistoryTable("__ef_migrations", AuthDbContext.Schema))
            .Options;

        return new AuthDbContext(options);
    }
}
