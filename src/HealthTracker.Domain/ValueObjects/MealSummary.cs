using HealthTracker.Domain.Enums;

namespace HealthTracker.Domain.ValueObjects;

public record MealSummary(MealType Meal, int Kcal, int EntryCount);
