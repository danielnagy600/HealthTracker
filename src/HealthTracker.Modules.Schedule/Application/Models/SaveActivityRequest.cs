namespace HealthTracker.Modules.Schedule.Application.Models;

public record SaveActivityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note);
