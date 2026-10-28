namespace HealthTracker.Modules.Water.Application.Dtos;

public record SettingsResponse(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
