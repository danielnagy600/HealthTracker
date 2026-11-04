namespace HealthTracker.Modules.Water.Application.Models;

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
