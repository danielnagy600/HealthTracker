namespace HealthTracker.Modules.Calories.Application.Models;

public record MealGroup(string Meal, int Kcal, int EntryCount, IReadOnlyList<FoodEntryItem> Entries);
