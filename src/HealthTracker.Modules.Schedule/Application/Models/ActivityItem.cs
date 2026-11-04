namespace HealthTracker.Modules.Schedule.Application.Models;

public record ActivityItem(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note,
    int DurationMinutes);
