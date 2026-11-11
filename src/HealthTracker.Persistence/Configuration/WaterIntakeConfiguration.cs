using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthTracker.Persistence.Configuration;

public sealed class WaterIntakeConfiguration : IEntityTypeConfiguration<WaterIntake>
{
    public void Configure(EntityTypeBuilder<WaterIntake> builder)
    {
        builder.ToTable("intakes", Schemas.Water);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AmountMl).IsRequired();
        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.RecordedAt).IsRequired();

        builder.HasIndex(x => new { x.UserId, x.Date });
    }
}
