namespace HealthTracker.Application.Features.Calories.Models;

public record MealGroup(string Meal, int Kcal, int EntryCount, IReadOnlyList<FoodEntryItem> Entries);
