using HealthTracker.Domain.Entities;
using HealthTracker.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Persistence;

// Az egyetlen DbContext: az Identity táblák (felhasználók, szerepkörök) és az összes
// funkció entitásai. A táblák továbbra is funkciónként külön PostgreSQL sémában
// vannak (identity / water / calories / schedule) - ezt a Configuration/ mappában
// lévő IEntityTypeConfiguration osztályok állítják be.
public sealed class HealthTrackerDbContext : IdentityDbContext<AppUser>
{
    public const string MigrationsHistorySchema = "public";
    public const string MigrationsHistoryTable = "__ef_migrations";

    public DbSet<WaterIntake> WaterIntakes => Set<WaterIntake>();
    public DbSet<WaterSettings> WaterSettings => Set<WaterSettings>();
    public DbSet<FoodEntry> FoodEntries => Set<FoodEntry>();
    public DbSet<CalorieGoal> CalorieGoals => Set<CalorieGoal>();
    public DbSet<Activity> Activities => Set<Activity>();

    public HealthTrackerDbContext(DbContextOptions<HealthTrackerDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Ami nem kap saját sémát, az ide kerül - ezek az Identity táblái (AspNetUsers ...).
        builder.HasDefaultSchema(Schemas.Identity);

        // Beolvassa az összes IEntityTypeConfiguration<T> osztályt ebből a projektből.
        builder.ApplyConfigurationsFromAssembly(typeof(HealthTrackerDbContext).Assembly);
    }
}
