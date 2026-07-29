namespace HealthTracker.Modules.Water.Application;

// Ezek a DTO-k (Data Transfer Object) a modul "külső szerződése": ezt kapja/küldi
// a HTTP-réteg. Szándékosan elválik a Domain entitásoktól, hogy a belső modell
// szabadon változhasson anélkül, hogy az API elromlana.

/// <summary>Új vízbejegyzés kérése.</summary>
public record AddIntakeRequest(int AmountMl);

/// <summary>Egyetlen bejegyzés a napi listában.</summary>
public record IntakeItem(Guid Id, DateTimeOffset RecordedAt, int AmountMl);

/// <summary>A mai nap összesítése.</summary>
public record DailySummaryResponse(
    DateOnly Date,
    int TargetMl,
    int ConsumedMl,
    int RemainingMl,
    double PercentComplete,
    IReadOnlyList<IntakeItem> Intakes);

/// <summary>Emlékeztető: hol tartasz, és mikor mennyit igyál.</summary>
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

/// <summary>A felhasználó beállításai.</summary>
public record SettingsResponse(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);

/// <summary>Beállítások módosítása.</summary>
public record UpdateSettingsRequest(int DailyTargetMl, TimeOnly WakeTime, TimeOnly SleepTime);
