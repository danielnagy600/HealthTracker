namespace HealthTracker.Modules.Calories.Application;

public record FoodEntryItem(
    Guid Id,
    DateOnly Date,
    string Meal,
    string Name,
    int Calories,
    DateTimeOffset RecordedAt);

public record SaveFoodEntryRequest(DateOnly Date, string Meal, string Name, int Calories);

public record MealGroup(string Meal, int Kcal, int EntryCount, IReadOnlyList<FoodEntryItem> Entries);

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

public record GoalResponse(int DailyTargetKcal);

public record UpdateGoalRequest(int DailyTargetKcal);
