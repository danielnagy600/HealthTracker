namespace HealthTracker.Modules.Schedule.Application.Models;

public record DayScheduleResponse(
    DateOnly Date,
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int BusyMinutes,
    int FreeMinutes,
    IReadOnlyList<ActivityItem> Activities,
    IReadOnlyList<TimeSlot> FreeSlots,
    IReadOnlyList<ConflictItem> Conflicts);
