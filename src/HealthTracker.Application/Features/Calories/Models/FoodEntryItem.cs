namespace HealthTracker.Application.Features.Calories.Models;

public record FoodEntryItem(
    Guid Id,
    DateOnly Date,
    string Meal,
    string Name,
    int Calories,
    DateTimeOffset RecordedAt);
