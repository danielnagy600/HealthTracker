namespace HealthTracker.Modules.Water.Application.Models;

public record UpdateSettingsRequest(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
