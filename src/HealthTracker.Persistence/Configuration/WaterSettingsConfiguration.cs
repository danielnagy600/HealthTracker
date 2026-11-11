using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthTracker.Persistence.Configuration;

public sealed class WaterSettingsConfiguration : IEntityTypeConfiguration<WaterSettings>
{
    public void Configure(EntityTypeBuilder<WaterSettings> builder)
    {
        builder.ToTable("settings", Schemas.Water);
        builder.HasKey(x => x.UserId);
    }
}
