namespace HealthTracker.Modules.Calories.Application;

// Ezek a DTO-k a modul "külső szerződése": ezt kapja/küldi a HTTP-réteg.
// Szándékosan elválik a Domain entitásoktól, hogy a belső modell szabadon
// változhasson anélkül, hogy az API elromlana.

/// <summary>Egy bejegyzés a napi listában. Az étkezés stringként megy (pl. "Breakfast").</summary>
public record FoodEntryItem(
    Guid Id,
    DateOnly Date,
    string Meal,
    string Name,
    int Calories,
    DateTimeOffset RecordedAt);

/// <summary>Bejegyzés létrehozása vagy módosítása – ugyanaz a mezőkészlet.</summary>
public record SaveFoodEntryRequest(DateOnly Date, string Meal, string Name, int Calories);

/// <summary>Egy étkezés szekciója a napi bontásban, a hozzá tartozó bejegyzésekkel.</summary>
public record MealGroup(string Meal, int Kcal, int EntryCount, IReadOnlyList<FoodEntryItem> Entries);

/// <summary>Egy nap teljes képe: a bejegyzések étkezésenként és a belőlük számolt egyenleg.</summary>
public record DayCaloriesResponse(
    DateOnly Date,
    int ConsumedKcal,
    int TargetKcal,
    int RemainingKcal,
    int OverKcal,
    double PercentOfTarget,
    string Status,
    string Message,
    string? LargestMeal,
    IReadOnlyList<MealGroup> Meals);

/// <summary>A felhasználó napi kalóriakerete.</summary>
public record GoalResponse(int DailyTargetKcal);

/// <summary>A napi keret módosítása.</summary>
public record UpdateGoalRequest(int DailyTargetKcal);
