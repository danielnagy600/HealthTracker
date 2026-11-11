using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthTracker.Persistence.Configuration;

public sealed class FoodEntryConfiguration : IEntityTypeConfiguration<FoodEntry>
{
    public void Configure(EntityTypeBuilder<FoodEntry> builder)
    {
        builder.ToTable("entries", Schemas.Calories);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(FoodEntry.MaxNameLength);
        builder.Property(x => x.Calories).IsRequired();
        builder.Property(x => x.RecordedAt).IsRequired();

        // Az enum szövegként kerül az adatbázisba ("Breakfast"), nem számként.
        builder.Property(x => x.Meal)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.HasIndex(x => new { x.UserId, x.Date });
    }
}
