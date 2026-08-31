namespace HealthTracker.Modules.Water.Domain;

public record WaterReminder(
    int ConsumedMl,
    int TargetMl,
    int RemainingMl,
    int ExpectedByNowMl,
    int DeficitMl,
    ReminderStatus Status,
    int NextDoseMl,
    DateTimeOffset? NextReminderAt,
    string Message);
