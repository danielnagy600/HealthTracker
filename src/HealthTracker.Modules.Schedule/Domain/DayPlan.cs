namespace HealthTracker.Modules.Schedule.Domain;

public record TimeRange(TimeOnly Start, TimeOnly End)
{
    public int DurationMinutes => (int)(End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;
}

public record ActivityConflict(Activity First, Activity Second, TimeRange Overlap);

public record DayPlan(
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<TimeRange> FreeSlots,
    IReadOnlyList<ActivityConflict> Conflicts);
