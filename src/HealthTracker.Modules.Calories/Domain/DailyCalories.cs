namespace HealthTracker.Modules.Calories.Domain;

public enum CalorieStatus
{
    Under,

    OnTarget,

    Over
}

public record MealSummary(MealType Meal, int Kcal, int EntryCount);

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
