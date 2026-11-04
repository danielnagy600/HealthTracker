namespace HealthTracker.Modules.Calories.Application.Models;

public record FoodEntryItem(
    Guid Id,
    DateOnly Date,
    string Meal,
    string Name,
    int Calories,
    DateTimeOffset RecordedAt);
