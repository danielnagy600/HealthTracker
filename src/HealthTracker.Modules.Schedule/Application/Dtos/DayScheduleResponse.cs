namespace HealthTracker.Modules.Schedule.Application.Dtos;

public record DayScheduleResponse(
    DateOnly Date,
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<ActivityItem> Activities,
    IReadOnlyList<TimeSlot> FreeSlots,
    IReadOnlyList<ConflictItem> Conflicts);
