namespace HealthTracker.Domain.ValueObjects;

public record DayPlan(
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<TimeRange> FreeSlots,
    IReadOnlyList<ActivityConflict> Conflicts);
