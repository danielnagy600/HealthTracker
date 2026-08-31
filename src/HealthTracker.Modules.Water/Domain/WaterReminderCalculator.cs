namespace HealthTracker.Modules.Water.Domain;

public static class WaterReminderCalculator
{
    private const int ToleranceMl = 150;

    private const int GlassMl = 250;

    public static WaterReminder Calculate(WaterSettings settings, int consumedMl, DateTimeOffset now)
    {
        int target = settings.DailyTargetMl;
        TimeOnly wake = settings.WakeTime;
        TimeOnly sleep = settings.SleepTime;
        TimeOnly nowTime = TimeOnly.FromTimeSpan(now.TimeOfDay);

        int totalAwakeMinutes = Math.Max(1, (int)(sleep.ToTimeSpan() - wake.ToTimeSpan()).TotalMinutes);
        int awakeHours = Math.Max(1, (int)Math.Round(totalAwakeMinutes / 60.0));

        int doseSize = Math.Max(GlassMl, (int)Math.Round((double)target / awakeHours / 50.0) * 50);

        int remaining = Math.Max(0, target - consumedMl);

        int minutesSinceWake = (int)(nowTime.ToTimeSpan() - wake.ToTimeSpan()).TotalMinutes;
        int elapsed = Math.Clamp(minutesSinceWake, 0, totalAwakeMinutes);
        double elapsedFraction = (double)elapsed / totalAwakeMinutes;
        int expectedByNow = (int)Math.Round(target * elapsedFraction);
        int deficit = Math.Max(0, expectedByNow - consumedMl);

        DateTimeOffset At(TimeOnly t) =>
            new(now.Year, now.Month, now.Day, t.Hour, t.Minute, 0, now.Offset);

        if (consumedMl >= target)
        {
            return new WaterReminder(consumedMl, target, remaining, expectedByNow, 0,
                ReminderStatus.GoalReached, 0, null,
                $"Well done! You've reached your daily goal of {target} ml. 🎉");
        }

        if (nowTime < wake)
        {
            return new WaterReminder(consumedMl, target, remaining, expectedByNow, deficit,
                ReminderStatus.OnTrack, doseSize, At(wake),
                $"Your hydration day starts at {wake:HH\\:mm}. First glass (~{doseSize} ml) then.");
        }

        if (nowTime >= sleep)
        {
            return new WaterReminder(consumedMl, target, remaining, expectedByNow, deficit,
                ReminderStatus.Behind, remaining, null,
                $"The day is over. You drank {consumedMl} of {target} ml. Try to finish earlier tomorrow!");
        }

        if (deficit > ToleranceMl)
        {
            int doseNow = Math.Min(remaining, Math.Max(deficit, GlassMl));
            return new WaterReminder(consumedMl, target, remaining, expectedByNow, deficit,
                ReminderStatus.Behind, doseNow, now,
                $"You're behind by ~{deficit} ml. Drink about {doseNow} ml now to catch up.");
        }

        TimeOnly nextHour = new(Math.Min(23, nowTime.Hour + 1), 0);
        DateTimeOffset nextAt;
        int nextDose;
        if (nextHour < sleep)
        {
            nextAt = At(nextHour);
            nextDose = Math.Min(remaining, doseSize);
        }
        else
        {
            nextAt = At(sleep);
            nextDose = remaining;
        }

        return new WaterReminder(consumedMl, target, remaining, expectedByNow, deficit,
            ReminderStatus.OnTrack, nextDose, nextAt,
            $"On track — {consumedMl}/{target} ml. Next glass (~{nextDose} ml) around {nextAt:HH\\:mm}.");
    }
}
