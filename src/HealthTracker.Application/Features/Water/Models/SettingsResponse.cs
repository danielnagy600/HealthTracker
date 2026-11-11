namespace HealthTracker.Application.Features.Water.Models;

public record SettingsResponse(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
