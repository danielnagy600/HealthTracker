namespace HealthTracker.Modules.Schedule.Application.Models;

public record TimeSlot(TimeOnly Start, TimeOnly End, int DurationMinutes);
