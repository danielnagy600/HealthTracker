using HealthTracker.Domain.Enums;

namespace HealthTracker.Domain.ValueObjects;

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
