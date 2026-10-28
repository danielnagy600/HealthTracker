namespace HealthTracker.Modules.Schedule.Application.Dtos;

public record SaveActivityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note);
