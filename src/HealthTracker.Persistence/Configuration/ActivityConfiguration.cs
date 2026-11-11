using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthTracker.Persistence.Configuration;

public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activities", Schemas.Schedule);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(Activity.MaxTitleLength);
        builder.Property(x => x.Note).HasMaxLength(Activity.MaxNoteLength);

        // Az enum szövegként kerül az adatbázisba ("Green"), nem számként.
        builder.Property(x => x.Color)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        // Számolt tulajdonság (EndTime - StartTime), nincs külön oszlopa.
        builder.Ignore(x => x.DurationMinutes);

        builder.HasIndex(x => new { x.UserId, x.Date });
    }
}
