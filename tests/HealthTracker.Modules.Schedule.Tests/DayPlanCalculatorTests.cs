using FluentAssertions;
using HealthTracker.Modules.Schedule.Domain;
using Xunit;

namespace HealthTracker.Modules.Schedule.Tests;

public class DayPlanCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 8, 16);
    private static readonly Guid User = Guid.NewGuid();

    private static Activity Act(int fromHour, int toHour, string title = "Teendő", int fromMinute = 0, int toMinute = 0) =>
        new(Guid.NewGuid(), User, Today,
            new TimeOnly(fromHour, fromMinute), new TimeOnly(toHour, toMinute),
            title, ActivityColor.Blue);

    [Fact]
    public void Empty_day_is_entirely_free()
    {
        var plan = DayPlanCalculator.Calculate([], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.BusyMinutes.Should().Be(0);
        plan.FreeMinutes.Should().Be(16 * 60);
        plan.FreeSlots.Should().ContainSingle();
        plan.Conflicts.Should().BeEmpty();
    }

    [Fact]
    public void Busy_and_free_minutes_add_up_to_the_window()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(8, 9), Act(13, 14)], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.BusyMinutes.Should().Be(120);
        plan.FreeMinutes.Should().Be(16 * 60 - 120);
        (plan.BusyMinutes + plan.FreeMinutes).Should().Be(16 * 60);
    }

    [Fact]
    public void Gaps_between_activities_are_reported_as_free_slots()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(8, 9), Act(13, 14)], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.FreeSlots.Should().HaveCount(3);
        plan.FreeSlots[0].Should().Be(new TimeRange(new TimeOnly(6, 0), new TimeOnly(8, 0)));
        plan.FreeSlots[1].Should().Be(new TimeRange(new TimeOnly(9, 0), new TimeOnly(13, 0)));
        plan.FreeSlots[2].Should().Be(new TimeRange(new TimeOnly(14, 0), new TimeOnly(22, 0)));
    }

    [Fact]
    public void Overlapping_activities_are_reported_as_a_conflict()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(9, 11, "Megbeszélés"), Act(10, 12, "Edzés")], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.Conflicts.Should().ContainSingle();
        var conflict = plan.Conflicts[0];
        conflict.First.Title.Should().Be("Megbeszélés");
        conflict.Second.Title.Should().Be("Edzés");
        conflict.Overlap.Should().Be(new TimeRange(new TimeOnly(10, 0), new TimeOnly(11, 0)));
        conflict.Overlap.DurationMinutes.Should().Be(60);
    }

    [Fact]
    public void Overlapping_time_is_not_counted_twice_as_busy()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(9, 11), Act(10, 12)], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.BusyMinutes.Should().Be(180);
    }

    [Fact]
    public void Touching_activities_do_not_conflict()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(8, 9), Act(9, 10)], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.Conflicts.Should().BeEmpty();
        plan.BusyMinutes.Should().Be(120);

        plan.FreeSlots.Should().HaveCount(2);
    }

    [Fact]
    public void An_activity_fully_inside_another_is_still_a_conflict()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(9, 17, "Konferencia"), Act(12, 13, "Ebéd")], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.Conflicts.Should().ContainSingle();
        plan.Conflicts[0].Overlap.Should().Be(new TimeRange(new TimeOnly(12, 0), new TimeOnly(13, 0)));
        plan.BusyMinutes.Should().Be(8 * 60);
    }

    [Fact]
    public void Three_way_overlap_reports_every_pair()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(9, 12), Act(10, 13), Act(11, 14)], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.Conflicts.Should().HaveCount(3);
        plan.BusyMinutes.Should().Be(5 * 60);
    }

    [Fact]
    public void Activities_outside_the_window_are_clipped()
    {
        var plan = DayPlanCalculator.Calculate(
            [Act(5, 7)], new TimeOnly(6, 0), new TimeOnly(22, 0));

        plan.BusyMinutes.Should().Be(60);
    }

    [Fact]
    public void Window_expands_to_fit_early_and_late_activities()
    {
        var (start, end) = DayPlanCalculator.WindowFor([Act(5, 7), Act(20, 23, fromMinute: 0, toMinute: 30)]);

        start.Should().Be(new TimeOnly(5, 0));
        end.Should().Be(new TimeOnly(23, 59));
    }

    [Fact]
    public void Window_stays_at_the_default_for_an_ordinary_day()
    {
        var (start, end) = DayPlanCalculator.WindowFor([Act(9, 17)]);

        start.Should().Be(DayPlanCalculator.DefaultWindowStart);
        end.Should().Be(DayPlanCalculator.DefaultWindowEnd);
    }

    [Fact]
    public void An_invalid_window_is_rejected()
    {
        var call = () => DayPlanCalculator.Calculate([], new TimeOnly(22, 0), new TimeOnly(6, 0));

        call.Should().Throw<ArgumentException>();
    }
}
