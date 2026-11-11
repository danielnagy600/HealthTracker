namespace HealthTracker.Application.Features.Schedule.Models;

public record TimeSlot(TimeOnly Start, TimeOnly End, int DurationMinutes);
