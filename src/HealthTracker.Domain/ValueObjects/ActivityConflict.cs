using HealthTracker.Domain.Entities;

namespace HealthTracker.Domain.ValueObjects;

public record ActivityConflict(Activity First, Activity Second, TimeRange Overlap);
