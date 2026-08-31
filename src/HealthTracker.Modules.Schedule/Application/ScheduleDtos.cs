namespace HealthTracker.Modules.Schedule.Application;

public record ActivityItem(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note,
    int DurationMinutes);

public record SaveActivityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note);

public record TimeSlot(TimeOnly Start, TimeOnly End, int DurationMinutes);

public record ConflictItem(
    Guid FirstId,
    Guid SecondId,
    string FirstTitle,
    string SecondTitle,
    TimeOnly OverlapStart,
    TimeOnly OverlapEnd,
    int OverlapMinutes);

public record DayScheduleResponse(
    DateOnly Date,
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<ActivityItem> Activities,
    IReadOnlyList<TimeSlot> FreeSlots,
    IReadOnlyList<ConflictItem> Conflicts);
