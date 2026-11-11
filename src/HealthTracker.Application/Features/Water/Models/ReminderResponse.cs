namespace HealthTracker.Application.Features.Water.Models;

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
