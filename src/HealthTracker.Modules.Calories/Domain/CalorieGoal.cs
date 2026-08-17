namespace HealthTracker.Modules.Calories.Domain;

/// <summary>
/// Egy felhasználó kalória-beállítása: a napi keret.
/// Felhasználónként egy sor létezik (a kulcs maga a UserId).
/// </summary>
public class CalorieGoal
{
    /// <summary>A legkisebb és legnagyobb értelmes napi keret.</summary>
    public const int MinTargetKcal = 500;
    public const int MaxTargetKcal = 10000;

    /// <summary>A tulajdonos felhasználó – egyben az elsődleges kulcs.</summary>
    public Guid UserId { get; set; }

    /// <summary>Napi kalóriakeret (alapértelmezés: 2000 kcal).</summary>
    public int DailyTargetKcal { get; set; } = 2000;

    /// <summary>Alapértelmezett beállítás létrehozása egy új felhasználónak.</summary>
    public static CalorieGoal CreateDefault(Guid userId) => new() { UserId = userId };

    /// <summary>A napi keret ellenőrzése; null, ha rendben van.</summary>
    public static string? Validate(int dailyTargetKcal) =>
        dailyTargetKcal is < MinTargetKcal or > MaxTargetKcal
            ? $"The daily target must be between {MinTargetKcal} and {MaxTargetKcal} kcal."
            : null;
}
