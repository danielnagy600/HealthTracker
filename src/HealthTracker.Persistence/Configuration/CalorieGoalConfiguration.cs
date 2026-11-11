using HealthTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HealthTracker.Persistence.Configuration;

public sealed class CalorieGoalConfiguration : IEntityTypeConfiguration<CalorieGoal>
{
    public void Configure(EntityTypeBuilder<CalorieGoal> builder)
    {
        builder.ToTable("goals", Schemas.Calories);
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.DailyTargetKcal).IsRequired();
    }
}
