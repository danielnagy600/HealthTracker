namespace HealthTracker.Modules.Calories.Application.Dtos;

public record MealGroup(string Meal, int Kcal, int EntryCount, IReadOnlyList<FoodEntryItem> Entries);
