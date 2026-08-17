using FluentAssertions;
using HealthTracker.Modules.Calories.Domain;
using Xunit;

namespace HealthTracker.Modules.Calories.Tests;

/// <summary>
/// A tiszta domain-logika tesztjei. Nincs adatbázis, nincs mock – csak a
/// bemenetből számolunk, ezért ezek a leggyorsabb és legstabilabb tesztek.
/// </summary>
public class CalorieCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 8, 16);
    private static readonly Guid User = Guid.NewGuid();
    private static readonly DateTimeOffset Noon = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    private static FoodEntry Entry(int kcal, MealType meal = MealType.Lunch, string name = "Étel") =>
        new(Guid.NewGuid(), User, Today, meal, name, kcal, Noon);

    [Fact]
    public void An_empty_day_leaves_the_whole_target_available()
    {
        var day = CalorieCalculator.Calculate([], 2000);

        day.ConsumedKcal.Should().Be(0);
        day.RemainingKcal.Should().Be(2000);
        day.OverKcal.Should().Be(0);
        day.Status.Should().Be(CalorieStatus.Under);
        day.LargestMeal.Should().BeNull();
        day.Message.Should().Contain("2000");
    }

    [Fact]
    public void Consumed_and_remaining_add_up_to_the_target()
    {
        var day = CalorieCalculator.Calculate([Entry(500), Entry(300)], 2000);

        day.ConsumedKcal.Should().Be(800);
        day.RemainingKcal.Should().Be(1200);
        (day.ConsumedKcal + day.RemainingKcal).Should().Be(2000);
    }

    [Fact]
    public void Staying_well_below_the_target_reports_under()
    {
        var day = CalorieCalculator.Calculate([Entry(800)], 2000);

        day.Status.Should().Be(CalorieStatus.Under);
        day.OverKcal.Should().Be(0);
        day.Message.Should().Contain("1200");
    }

    [Fact]
    public void Landing_just_below_the_target_reports_on_target()
    {
        // 1950 / 2000: a 100 kcal-os tűréshatáron belül van.
        var day = CalorieCalculator.Calculate([Entry(1950)], 2000);

        day.Status.Should().Be(CalorieStatus.OnTarget);
        day.RemainingKcal.Should().Be(50);
        day.OverKcal.Should().Be(0);
    }

    [Fact]
    public void Hitting_the_target_exactly_is_on_target()
    {
        var day = CalorieCalculator.Calculate([Entry(2000)], 2000);

        day.Status.Should().Be(CalorieStatus.OnTarget);
        day.RemainingKcal.Should().Be(0);
        day.OverKcal.Should().Be(0);
    }

    [Fact]
    public void Exceeding_the_target_reports_how_much_over()
    {
        var day = CalorieCalculator.Calculate([Entry(1500), Entry(900)], 2000);

        day.Status.Should().Be(CalorieStatus.Over);
        day.OverKcal.Should().Be(400);
        day.RemainingKcal.Should().Be(0); // nem megy negatívba
        day.Message.Should().Contain("400");
    }

    [Fact]
    public void Percent_of_target_is_rounded_to_one_decimal()
    {
        var day = CalorieCalculator.Calculate([Entry(667)], 2000);

        day.PercentOfTarget.Should().Be(33.4);
    }

    [Fact]
    public void Percent_can_exceed_one_hundred()
    {
        var day = CalorieCalculator.Calculate([Entry(3000)], 2000);

        day.PercentOfTarget.Should().Be(150);
    }

    [Fact]
    public void Every_meal_appears_in_the_breakdown_even_when_empty()
    {
        var day = CalorieCalculator.Calculate([Entry(400, MealType.Breakfast)], 2000);

        day.Meals.Should().HaveCount(4);
        day.Meals.Select(m => m.Meal).Should().ContainInOrder(
            MealType.Breakfast, MealType.Lunch, MealType.Dinner, MealType.Snack);

        day.Meals.Single(m => m.Meal == MealType.Breakfast).Kcal.Should().Be(400);
        day.Meals.Single(m => m.Meal == MealType.Dinner).Kcal.Should().Be(0);
        day.Meals.Single(m => m.Meal == MealType.Dinner).EntryCount.Should().Be(0);
    }

    [Fact]
    public void Entries_are_summed_per_meal()
    {
        var day = CalorieCalculator.Calculate(
            [
                Entry(320, MealType.Breakfast),
                Entry(100, MealType.Breakfast),
                Entry(650, MealType.Lunch)
            ],
            2000);

        day.Meals.Single(m => m.Meal == MealType.Breakfast).Kcal.Should().Be(420);
        day.Meals.Single(m => m.Meal == MealType.Breakfast).EntryCount.Should().Be(2);
        day.Meals.Single(m => m.Meal == MealType.Lunch).Kcal.Should().Be(650);
    }

    [Fact]
    public void The_largest_meal_is_reported()
    {
        var day = CalorieCalculator.Calculate(
            [
                Entry(420, MealType.Breakfast),
                Entry(780, MealType.Lunch),
                Entry(450, MealType.Dinner)
            ],
            2000);

        day.LargestMeal.Should().NotBeNull();
        day.LargestMeal!.Meal.Should().Be(MealType.Lunch);
        day.LargestMeal.Kcal.Should().Be(780);
    }

    [Fact]
    public void A_zero_target_does_not_divide_by_zero()
    {
        var day = CalorieCalculator.Calculate([Entry(500)], 0);

        day.Status.Should().Be(CalorieStatus.Over);
        day.PercentOfTarget.Should().BeGreaterThan(0);
    }
}
