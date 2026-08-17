namespace HealthTracker.Modules.Schedule.Domain;

/// <summary>
/// A modul üzleti magja: a nap elfoglaltságaiból kiszámolja a lefoglalt és a
/// szabad időt, a szabad réseket, valamint az egymással ütköző elfoglaltságokat.
///
/// Szándékosan <b>statikus, tiszta függvény</b>: nincs adatbázis, nincs óra-hívás,
/// csak a bemenetből számol – ezért egységtesztben triviálisan ellenőrizhető.
/// </summary>
public static class DayPlanCalculator
{
    /// <summary>Az alapértelmezett napi ablak, ha nincs semmi a naptárban.</summary>
    public static readonly TimeOnly DefaultWindowStart = new(6, 0);
    public static readonly TimeOnly DefaultWindowEnd = new(22, 0);

    /// <summary>
    /// Az idővonal megjelenítendő ablaka: az alapértelmezett 06:00–22:00, de
    /// kitágítva, hogy a korábbi vagy későbbi elfoglaltságok is beleférjenek
    /// (egész órára kerekítve). Így a felületen soha nem lóg ki semmi.
    /// </summary>
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
                // Felfelé kerekítés egész órára, 23:59-nél megállva.
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

        // A lefoglalt idő az összevont sávokból jön, így az átfedés nem számít duplán.
        int busy = merged.Sum(r => r.DurationMinutes);
        var freeSlots = GapsBetween(merged, windowStart, windowEnd);
        int free = freeSlots.Sum(r => r.DurationMinutes);

        return new DayPlan(windowStart, windowEnd, busy, free, freeSlots, conflicts);
    }

    /// <summary>Minden olyan pár, amely időben átfed. Az érintkezés (10:00-ig / 10:00-tól) nem ütközés.</summary>
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

                // Rendezett lista: ha a következő a jelenlegi vége után kezdődik,
                // akkor az összes utána következő is – mehetünk a következő i-re.
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

    /// <summary>Az elfoglaltságok ablakra vágva és átfedés mentén összevonva.</summary>
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
                // Átfed vagy érintkezik az előzővel – kiterjesztjük.
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

    /// <summary>Az összevont sávok közötti (és a szélein maradó) szabad rések.</summary>
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
