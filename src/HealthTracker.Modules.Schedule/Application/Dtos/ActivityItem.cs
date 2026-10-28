namespace HealthTracker.Modules.Schedule.Application.Dtos;

public record ActivityItem(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note,
    int DurationMinutes);
