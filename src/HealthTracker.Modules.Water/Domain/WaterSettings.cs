namespace HealthTracker.Modules.Water.Domain;

/// <summary>
/// Egy felhasználó vízfogyasztási beállításai: napi cél és ébrenléti időablak.
/// Felhasználónként egy sor létezik (a kulcs maga a UserId).
/// </summary>
public class WaterSettings
{
    /// <summary>A tulajdonos felhasználó – egyben az elsődleges kulcs.</summary>
    public Guid UserId { get; set; }

    /// <summary>Napi cél milliliterben (alapértelmezés: 2000 ml).</summary>
    public int DailyTargetMl { get; set; } = 2000;

    /// <summary>Ébredés ideje – ekkortól ajánljuk az ivást.</summary>
    public TimeOnly WakeTime { get; set; } = new(7, 0);

    /// <summary>Lefekvés ideje – eddig kellene teljesíteni a napi célt.</summary>
    public TimeOnly SleepTime { get; set; } = new(22, 0);

    /// <summary>Alapértelmezett beállítások létrehozása egy új felhasználónak.</summary>
    public static WaterSettings CreateDefault(Guid userId) => new() { UserId = userId };
}
