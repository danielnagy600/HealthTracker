namespace HealthTracker.Modules.Water.Application;

public record AddIntakeRequest(int AmountMl);

public record IntakeItem(Guid Id, DateTimeOffset RecordedAt, int AmountMl);

public record DailySummaryResponse(
    DateOnly Date,
    int TargetMl,
    int ConsumedMl,
    int RemainingMl,
    double PercentComplete,
    IReadOnlyList<IntakeItem> Intakes);

public record ReminderResponse(
    int ConsumedMl,
    int TargetMl,
    int RemainingMl,
    int ExpectedByNowMl,
    int DeficitMl,
    string Status,
    int NextDoseMl,
    DateTimeOffset? NextReminderAt,
    string Message);

public record SettingsResponse(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);

public record UpdateSettingsRequest(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
