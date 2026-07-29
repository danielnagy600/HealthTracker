using FluentAssertions;
using HealthTracker.Modules.Water.Domain;
using Xunit;

namespace HealthTracker.Modules.Water.Tests;

/// <summary>
/// A tiszta domain-logika tesztjei. Nincs adatbázis, nincs mock – csak a
/// bemenetből számolunk, ezért ezek a leggyorsabb és legstabilabb tesztek.
/// </summary>
public class WaterReminderCalculatorTests
{
    // Napi 2000 ml, ébrenlét 07:00–22:00 (15 óra).
    private static WaterSettings Settings() => new()
    {
        UserId = Guid.NewGuid(),
        DailyTargetMl = 2000,
        WakeTime = new(7, 0),
        SleepTime = new(22, 0)
    };

    private static DateTimeOffset At(int hour, int minute = 0) =>
        new(2026, 7, 29, hour, minute, 0, TimeSpan.Zero);

    [Fact]
    public void Reaching_the_target_reports_goal_reached()
    {
        var r = WaterReminderCalculator.Calculate(Settings(), consumedMl: 2000, now: At(15));

        r.Status.Should().Be(ReminderStatus.GoalReached);
        r.RemainingMl.Should().Be(0);
        r.NextReminderAt.Should().BeNull();
    }

    [Fact]
    public void Falling_behind_schedule_warns_to_drink_now()
    {
        // 15:00-kor időarányosan ~1067 ml-nél kellene tartani, de csak 200-at ivott.
        var r = WaterReminderCalculator.Calculate(Settings(), consumedMl: 200, now: At(15));

        r.Status.Should().Be(ReminderStatus.Behind);
        r.DeficitMl.Should().BeGreaterThan(0);
        r.NextDoseMl.Should().BeGreaterThan(0);
        r.NextReminderAt.Should().Be(At(15)); // most kell inni
    }

    [Fact]
    public void On_schedule_suggests_the_next_hourly_glass()
    {
        var r = WaterReminderCalculator.Calculate(Settings(), consumedMl: 1100, now: At(15));

        r.Status.Should().Be(ReminderStatus.OnTrack);
        r.NextReminderAt!.Value.Hour.Should().Be(16);
    }

    [Fact]
    public void Before_wake_time_points_to_the_first_glass_at_wake()
    {
        var r = WaterReminderCalculator.Calculate(Settings(), consumedMl: 0, now: At(6));

        r.Status.Should().Be(ReminderStatus.OnTrack);
        r.NextReminderAt!.Value.Hour.Should().Be(7);
    }

    [Fact]
    public void After_bedtime_without_reaching_goal_reports_behind()
    {
        var r = WaterReminderCalculator.Calculate(Settings(), consumedMl: 1500, now: At(23));

        r.Status.Should().Be(ReminderStatus.Behind);
        r.RemainingMl.Should().Be(500);
        r.NextReminderAt.Should().BeNull();
    }
}
