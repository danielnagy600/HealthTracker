using HealthTracker.Domain.Enums;

namespace HealthTracker.Application.Features.Calories;

// Az API stringként kapja az étkezés típusát ("breakfast", "Lunch" ...).
public static class MealTypeParser
{
    /// <summary>Kis-nagybetűtől függetlenül értelmez; ismeretlen értékre Snack.</summary>
    public static MealType Parse(string? meal) =>
        Enum.TryParse<MealType>(meal, ignoreCase: true, out var parsed) ? parsed : MealType.Snack;

    public static bool IsKnown(string? meal) =>
        Enum.TryParse<MealType>(meal, ignoreCase: true, out _);
}
