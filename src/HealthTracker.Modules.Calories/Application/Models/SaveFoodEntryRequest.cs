namespace HealthTracker.Modules.Calories.Application.Models;

public record SaveFoodEntryRequest(DateOnly Date, string Meal, string Name, int Calories);
