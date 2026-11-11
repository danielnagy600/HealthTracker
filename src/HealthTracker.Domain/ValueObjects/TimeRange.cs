namespace HealthTracker.Domain.ValueObjects;

public record TimeRange(TimeOnly Start, TimeOnly End)
{
    public int DurationMinutes => (int)(End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;
}
