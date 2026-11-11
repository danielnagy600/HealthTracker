using HealthTracker.Domain.Enums;

namespace HealthTracker.Application.Features.Schedule;

// Az API stringként kapja a színt ("green", "Blue" ...).
public static class ActivityColorParser
{
    /// <summary>Kis-nagybetűtől függetlenül értelmez; ismeretlen értékre Blue.</summary>
    public static ActivityColor Parse(string? color) =>
        Enum.TryParse<ActivityColor>(color, ignoreCase: true, out var parsed) ? parsed : ActivityColor.Blue;

    public static bool IsKnown(string? color) =>
        Enum.TryParse<ActivityColor>(color, ignoreCase: true, out _);
}
