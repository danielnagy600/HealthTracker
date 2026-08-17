namespace HealthTracker.Modules.Calories.Domain;

/// <summary>
/// A modul üzleti magja: a nap bejegyzéseiből kiszámolja az egyenleget, az
/// étkezésenkénti bontást és azt, hogy hol tartasz a napi kerethez képest.
///
/// Szándékosan <b>statikus, tiszta függvény</b>: nincs adatbázis, nincs óra-hívás,
/// csak a bemenetből számol – ezért egységtesztben triviálisan ellenőrizhető.
/// </summary>
public static class CalorieCalculator
{
    /// <summary>Ennyivel a keret alatt már "épp jó"-nak számít a nap.</summary>
    public const int ToleranceKcal = 100;

    public static DailyCalories Calculate(IReadOnlyList<FoodEntry> entries, int targetKcal)
    {
        int target = Math.Max(1, targetKcal);
        int consumed = entries.Sum(e => e.Calories);
        int remaining = Math.Max(0, target - consumed);
        int over = Math.Max(0, consumed - target);
        double percent = Math.Round(100.0 * consumed / target, 1);

        // Minden étkezés szerepel a bontásban, akkor is, ha még üres – így a
        // felületen nem ugrálnak a szekciók, ahogy bekerülnek a bejegyzések.
        var meals = Enum.GetValues<MealType>()
            .Select(meal =>
            {
                var forMeal = entries.Where(e => e.Meal == meal).ToList();
                return new MealSummary(meal, forMeal.Sum(e => e.Calories), forMeal.Count);
            })
            .ToList();

        var largestMeal = meals
            .Where(m => m.Kcal > 0)
            .OrderByDescending(m => m.Kcal)
            .ThenBy(m => m.Meal)
            .FirstOrDefault();

        var status = consumed > target
            ? CalorieStatus.Over
            : consumed >= target - ToleranceKcal
                ? CalorieStatus.OnTarget
                : CalorieStatus.Under;

        var message = BuildMessage(status, consumed, target, remaining, over, entries.Count);

        return new DailyCalories(
            consumed, target, remaining, over, percent, status, meals, largestMeal, message);
    }

    private static string BuildMessage(
        CalorieStatus status, int consumed, int target, int remaining, int over, int entryCount)
    {
        if (entryCount == 0)
            return $"Nothing logged today. Your daily target is {target} kcal.";

        return status switch
        {
            CalorieStatus.Over =>
                $"You're {over} kcal over your daily target ({consumed} / {target} kcal).",
            CalorieStatus.OnTarget =>
                $"Nicely within target — {consumed} / {target} kcal, {remaining} kcal left.",
            _ =>
                $"You have {remaining} kcal left today ({consumed} / {target} kcal)."
        };
    }
}
