using HealthTracker.Modules.Schedule.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Schedule.Infrastructure;

/// <summary>
/// A Schedule modul EF Core adatbázis-kontextusa. Saját PostgreSQL sémában
/// ("schedule") él, elkülönítve a többi modultól – ez a moduláris monolit egyik
/// jó gyakorlata: minden modulnak megvan a maga adat-területe.
/// </summary>
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

            // A színt olvasható néven tároljuk (nem sorszámként), így az adatbázis
            // önmagában is értelmezhető marad, és az enum bővítése nem tolja el a régi sorokat.
            e.Property(x => x.Color)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            // A számított tulajdonságot nem tároljuk – futásidőben jön a két időpontból.
            e.Ignore(x => x.DurationMinutes);

            // A napi lekérdezés (felhasználó + nap) gyors legyen.
            e.HasIndex(x => new { x.UserId, x.Date });
        });
    }
}
