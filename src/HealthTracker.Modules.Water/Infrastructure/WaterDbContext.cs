using HealthTracker.Modules.Water.Domain;
using Microsoft.EntityFrameworkCore;

namespace HealthTracker.Modules.Water.Infrastructure;

/// <summary>
/// A Water modul EF Core adatbázis-kontextusa. Saját PostgreSQL sémában ("water")
/// él, elkülönítve a többi modultól – ez a moduláris monolit egyik jó gyakorlata:
/// minden modulnak megvan a maga adat-területe.
/// </summary>
public sealed class WaterDbContext : DbContext
{
    public const string Schema = "water";

    public DbSet<WaterIntake> Intakes => Set<WaterIntake>();
    public DbSet<WaterSettings> Settings => Set<WaterSettings>();

    public WaterDbContext(DbContextOptions<WaterDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<WaterIntake>(e =>
        {
            e.ToTable("intakes");
            e.HasKey(x => x.Id);
            e.Property(x => x.AmountMl).IsRequired();
            e.Property(x => x.Date).IsRequired();
            e.Property(x => x.RecordedAt).IsRequired();
            // A napi lekérdezés (felhasználó + nap) gyors legyen.
            e.HasIndex(x => new { x.UserId, x.Date });
        });

        modelBuilder.Entity<WaterSettings>(e =>
        {
            e.ToTable("settings");
            e.HasKey(x => x.UserId); // felhasználónként egy sor
        });
    }
}
