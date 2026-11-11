namespace HealthTracker.Application.Features.Calories.Models;

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
