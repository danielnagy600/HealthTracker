namespace HealthTracker.Modules.Calories.Domain;

/// <summary>Hol tartasz a napi kerethez képest.</summary>
public enum CalorieStatus
{
    /// <summary>Bőven belefér még a keretbe.</summary>
    Under,

    /// <summary>Épp a keret körül vagy – ideális.</summary>
    OnTarget,

    /// <summary>Túllépted a napi keretet.</summary>
    Over
}

/// <summary>Egy étkezés összesítése.</summary>
public record MealSummary(MealType Meal, int Kcal, int EntryCount);

/// <summary>
/// A nap kiszámolt képe: mennyit ettél, mennyi fér még bele, hogyan oszlik meg
/// az étkezések között, és hol tartasz a kerethez képest.
/// </summary>
public record DailyCalories(
    int ConsumedKcal,
    int TargetKcal,
    int RemainingKcal,
    int OverKcal,
    double PercentOfTarget,
    CalorieStatus Status,
    IReadOnlyList<MealSummary> Meals,
    MealSummary? LargestMeal,
    string Message);
