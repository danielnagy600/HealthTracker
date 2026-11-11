namespace HealthTracker.Application.Features.Calories.Models;

public record SaveFoodEntryRequest(DateOnly Date, string Meal, string Name, int Calories);
