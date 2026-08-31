using HealthTracker.Modules.Schedule.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Schedule.Infrastructure;

public sealed class ScheduleDbContext : DbContext
{
    public const string Schema = "schedule";

    public DbSet<Activity> Activities => Set<Activity>();

    public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<Activity>(e =>
        {
            e.ToTable("activities");
            e.HasKey(x => x.Id);

            e.Property(x => x.Date).IsRequired();
            e.Property(x => x.StartTime).IsRequired();
            e.Property(x => x.EndTime).IsRequired();
            e.Property(x => x.Title).IsRequired().HasMaxLength(Activity.MaxTitleLength);
            e.Property(x => x.Note).HasMaxLength(Activity.MaxNoteLength);

            e.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            e.Ignore(x => x.DurationMinutes);

            e.HasIndex(x => new { x.UserId, x.Date });
        });
    }
}
