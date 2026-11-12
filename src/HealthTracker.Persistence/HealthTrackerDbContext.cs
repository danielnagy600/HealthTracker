using HealthTracker.Domain.Entities;
using HealthTracker.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Persistence;

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
        builder.HasDefaultSchema(Schemas.Identity);
        builder.ApplyConfigurationsFromAssembly(typeof(HealthTrackerDbContext).Assembly);
    }
}
