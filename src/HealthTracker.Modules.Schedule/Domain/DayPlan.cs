namespace HealthTracker.Modules.Schedule.Domain;

/// <summary>Egy összefüggő idősáv (pl. szabad rés a napban).</summary>
public record TimeRange(TimeOnly Start, TimeOnly End)
{
    public int DurationMinutes => (int)(End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;
}

/// <summary>Két elfoglaltság időbeli ütközése.</summary>
public record ActivityConflict(Activity First, Activity Second, TimeRange Overlap);

/// <summary>
/// A nap kiszámolt képe: mennyi a lefoglalt és a szabad idő, hol vannak a rések,
/// és mely elfoglaltságok ütköznek egymással.
/// </summary>
public record DayPlan(
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<TimeRange> FreeSlots,
    IReadOnlyList<ActivityConflict> Conflicts);
