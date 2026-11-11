namespace HealthTracker.Application.Features.Water.Models;

public record UpdateSettingsRequest(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
