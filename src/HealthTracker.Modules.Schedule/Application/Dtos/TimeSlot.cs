namespace HealthTracker.Modules.Schedule.Application.Dtos;

public record TimeSlot(TimeOnly Start, TimeOnly End, int DurationMinutes);
