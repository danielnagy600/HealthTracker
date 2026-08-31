namespace HealthTracker.Modules.Schedule.Domain;

public static class DayPlanCalculator
{
    public static readonly TimeOnly DefaultWindowStart = new(6, 0);
    public static readonly TimeOnly DefaultWindowEnd = new(22, 0);

    public static (TimeOnly Start, TimeOnly End) WindowFor(IReadOnlyList<Activity> activities)
    {
        var start = DefaultWindowStart;
        var end = DefaultWindowEnd;

        foreach (var activity in activities)
        {
            if (activity.StartTime < start)
                start = new TimeOnly(activity.StartTime.Hour, 0);

            if (activity.EndTime > end)
            {
                end = activity.EndTime.Minute == 0
                    ? activity.EndTime
                    : activity.EndTime.Hour >= 23
                        ? new TimeOnly(23, 59)
                        : new TimeOnly(activity.EndTime.Hour + 1, 0);
            }
        }

        return (start, end);
    }

    public static DayPlan Calculate(IReadOnlyList<Activity> activities, TimeOnly windowStart, TimeOnly windowEnd)
    {
        if (windowEnd <= windowStart)
            throw new ArgumentException("The window end must be after its start.", nameof(windowEnd));

        var conflicts = FindConflicts(activities);
        var merged = MergeIntoWindow(activities, windowStart, windowEnd);

        int busy = merged.Sum(r => r.DurationMinutes);
        var freeSlots = GapsBetween(merged, windowStart, windowEnd);
        int free = freeSlots.Sum(r => r.DurationMinutes);

        return new DayPlan(windowStart, windowEnd, busy, free, freeSlots, conflicts);
    }

    private static List<ActivityConflict> FindConflicts(IReadOnlyList<Activity> activities)
    {
        var ordered = activities.OrderBy(a => a.StartTime).ThenBy(a => a.EndTime).ToList();
        var conflicts = new List<ActivityConflict>();

        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = i + 1; j < ordered.Count; j++)
            {
                var first = ordered[i];
                var second = ordered[j];

                if (second.StartTime >= first.EndTime)
                    break;

                var overlapStart = Max(first.StartTime, second.StartTime);
                var overlapEnd = Min(first.EndTime, second.EndTime);
                if (overlapEnd > overlapStart)
                    conflicts.Add(new ActivityConflict(first, second, new TimeRange(overlapStart, overlapEnd)));
            }
        }

        return conflicts;
    }

    private static List<TimeRange> MergeIntoWindow(
        IReadOnlyList<Activity> activities, TimeOnly windowStart, TimeOnly windowEnd)
    {
        var clipped = activities
            .Select(a => new TimeRange(Max(a.StartTime, windowStart), Min(a.EndTime, windowEnd)))
            .Where(r => r.End > r.Start)
            .OrderBy(r => r.Start)
            .ToList();

        var merged = new List<TimeRange>();
        foreach (var range in clipped)
        {
            if (merged.Count > 0 && range.Start <= merged[^1].End)
            {
                if (range.End > merged[^1].End)
                    merged[^1] = merged[^1] with { End = range.End };
            }
            else
            {
                merged.Add(range);
            }
        }

        return merged;
    }

    private static List<TimeRange> GapsBetween(List<TimeRange> busy, TimeOnly windowStart, TimeOnly windowEnd)
    {
        var gaps = new List<TimeRange>();
        var cursor = windowStart;

        foreach (var range in busy)
        {
            if (range.Start > cursor)
                gaps.Add(new TimeRange(cursor, range.Start));

            if (range.End > cursor)
                cursor = range.End;
        }

        if (cursor < windowEnd)
            gaps.Add(new TimeRange(cursor, windowEnd));

        return gaps;
    }

    private static TimeOnly Max(TimeOnly a, TimeOnly b) => a > b ? a : b;

    private static TimeOnly Min(TimeOnly a, TimeOnly b) => a < b ? a : b;
}
