namespace HealthTracker.Modules.Calories.Application.Dtos;

public record SaveFoodEntryRequest(DateOnly Date, string Meal, string Name, int Calories);
