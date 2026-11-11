namespace HealthTracker.Application.Features.Schedule.Models;

public record SaveActivityRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Title,
    string Color,
    string? Note);
