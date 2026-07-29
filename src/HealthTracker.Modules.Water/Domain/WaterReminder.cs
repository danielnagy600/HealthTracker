namespace HealthTracker.Modules.Water.Domain;

/// <summary>
/// Az emlékeztető-számítás eredménye (érték-objektum). Megmondja, hogy hol tartasz,
/// mennyi van hátra, és mikor mennyit igyál legközelebb.
/// </summary>
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
