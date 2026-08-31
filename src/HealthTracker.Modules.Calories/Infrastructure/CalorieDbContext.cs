using HealthTracker.Modules.Calories.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Calories.Infrastructure;

public sealed class CalorieDbContext : DbContext
{
    public const string Schema = "calories";

    public DbSet<FoodEntry> Entries => Set<FoodEntry>();
    public DbSet<CalorieGoal> Goals => Set<CalorieGoal>();

    public CalorieDbContext(DbContextOptions<CalorieDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<FoodEntry>(e =>
        {
            e.ToTable("entries");
            e.HasKey(x => x.Id);

            e.Property(x => x.Date).IsRequired();
            e.Property(x => x.Name).IsRequired().HasMaxLength(FoodEntry.MaxNameLength);
            e.Property(x => x.Calories).IsRequired();
            e.Property(x => x.RecordedAt).IsRequired();

            e.Property(x => x.Meal)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            e.HasIndex(x => new { x.UserId, x.Date });
        });

        modelBuilder.Entity<CalorieGoal>(e =>
        {
            e.ToTable("goals");
            e.HasKey(x => x.UserId);
            e.Property(x => x.DailyTargetKcal).IsRequired();
        });
    }
}
