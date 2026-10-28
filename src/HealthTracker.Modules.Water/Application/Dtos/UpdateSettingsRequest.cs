namespace HealthTracker.Modules.Water.Application.Dtos;

public record UpdateSettingsRequest(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
