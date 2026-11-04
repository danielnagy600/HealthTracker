namespace HealthTracker.Modules.Water.Application.Models;

public record SettingsResponse(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
