using FluentAssertions;
using HealthTracker.Modules.Calories.Application;
using HealthTracker.Modules.Calories.Domain;
using HealthTracker.SharedKernel.Abstractions;
using Moq;
using Xunit;

namespace HealthTracker.Modules.Calories.Tests;

public class CalorieServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset Now = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 8, 16);

    private readonly Mock<IFoodEntryRepository> _repository = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly CalorieService _sut;

    public CalorieServiceTests()
    {
        _clock.SetupGet(c => c.Now).Returns(Now);
        _currentUser.SetupGet(c => c.UserId).Returns(UserId);

        _currentUser.Setup(c => c.RequireUserId()).Returns(UserId);
        _repository
            .Setup(r => r.GetOrCreateGoalAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalorieGoal.CreateDefault(UserId));

        _sut = new CalorieService(_repository.Object, _clock.Object, _currentUser.Object);
    }

    private static FoodEntry Entry(int kcal, MealType meal = MealType.Lunch, string name = "Étel") =>
        new(Guid.NewGuid(), UserId, Today, meal, name, kcal, Now);

    private static SaveFoodEntryRequest Request(
        string name = "Zabkása", int kcal = 320, string meal = "Breakfast") =>
        new(Today, meal, name, kcal);

    [Fact]
    public async Task GetDay_without_a_date_uses_today_from_the_clock()
    {
        _repository
            .Setup(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var day = await _sut.GetDayAsync();

        day.Date.Should().Be(Today);
        day.TargetKcal.Should().Be(2000);
        _repository.Verify(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDay_groups_entries_by_meal()
    {
        _repository
            .Setup(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                Entry(320, MealType.Breakfast, "Zabkása"),
                Entry(100, MealType.Breakfast, "Kávé"),
                Entry(650, MealType.Lunch, "Csirkemell")
            ]);

        var day = await _sut.GetDayAsync(Today);

        day.Meals.Should().HaveCount(4);
        var breakfast = day.Meals.Single(m => m.Meal == "Breakfast");
        breakfast.Kcal.Should().Be(420);
        breakfast.Entries.Select(e => e.Name).Should().ContainInOrder("Zabkása", "Kávé");

        day.Meals.Single(m => m.Meal == "Snack").Entries.Should().BeEmpty();
        day.ConsumedKcal.Should().Be(1070);
        day.LargestMeal.Should().Be("Lunch");
    }

    [Fact]
    public async Task Add_saves_the_entry_for_the_current_user_with_the_clock_time()
    {
        FoodEntry? saved = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<FoodEntry>(), It.IsAny<CancellationToken>()))
            .Callback<FoodEntry, CancellationToken>((e, _) => saved = e)
            .Returns(Task.CompletedTask);

        var result = await _sut.AddAsync(Request());

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(UserId);
        saved.Meal.Should().Be(MealType.Breakfast);
        saved.Calories.Should().Be(320);
        saved.RecordedAt.Should().Be(Now);
        result.Name.Should().Be("Zabkása");
        result.Meal.Should().Be("Breakfast");
    }

    [Fact]
    public async Task Add_trims_the_name()
    {
        FoodEntry? saved = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<FoodEntry>(), It.IsAny<CancellationToken>()))
            .Callback<FoodEntry, CancellationToken>((e, _) => saved = e)
            .Returns(Task.CompletedTask);

        await _sut.AddAsync(Request(name: "  Alma  "));

        saved!.Name.Should().Be("Alma");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(20000)]
    public async Task Add_rejects_an_invalid_calorie_value(int kcal)
    {
        var call = async () => await _sut.AddAsync(Request(kcal: kcal));

        await call.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Add_rejects_an_empty_name()
    {
        var call = async () => await _sut.AddAsync(Request(name: "   "));

        await call.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Update_changes_an_existing_entry()
    {
        var existing = Entry(320, MealType.Breakfast, "Zabkása");
        _repository
            .Setup(r => r.FindAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(existing.Id, Request(name: "Müzli", kcal: 410, meal: "Snack"));

        result.Should().NotBeNull();
        result!.Name.Should().Be("Müzli");
        result.Calories.Should().Be(410);
        result.Meal.Should().Be("Snack");
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_returns_null_for_someone_elses_entry()
    {
        var foreignId = Guid.NewGuid();

        _repository
            .Setup(r => r.FindAsync(UserId, foreignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FoodEntry?)null);

        var result = await _sut.UpdateAsync(foreignId, Request());

        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<FoodEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_removes_an_existing_entry()
    {
        var existing = Entry(320);
        _repository
            .Setup(r => r.FindAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var deleted = await _sut.DeleteAsync(existing.Id);

        deleted.Should().BeTrue();
        _repository.Verify(r => r.RemoveAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_returns_false_when_there_is_nothing_to_delete()
    {
        _repository
            .Setup(r => r.FindAsync(UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FoodEntry?)null);

        var deleted = await _sut.DeleteAsync(Guid.NewGuid());

        deleted.Should().BeFalse();
        _repository.Verify(r => r.RemoveAsync(It.IsAny<FoodEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task The_goal_can_be_changed()
    {
        var result = await _sut.UpdateGoalAsync(new UpdateGoalRequest(2500));

        result.DailyTargetKcal.Should().Be(2500);
        _repository.Verify(
            r => r.UpdateGoalAsync(It.Is<CalorieGoal>(g => g.DailyTargetKcal == 2500), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(50000)]
    public async Task An_unreasonable_goal_is_rejected(int kcal)
    {
        var call = async () => await _sut.UpdateGoalAsync(new UpdateGoalRequest(kcal));

        await call.Should().ThrowAsync<ArgumentException>();
        _repository.Verify(
            r => r.UpdateGoalAsync(It.IsAny<CalorieGoal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Every_query_is_scoped_to_the_logged_in_user()
    {
        _repository
            .Setup(r => r.GetForDateAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.GetDayAsync(Today);

        _repository.Verify(
            r => r.GetForDateAsync(OtherUserId, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("Breakfast", "Breakfast")]
    [InlineData("lunch", "Lunch")]
    [InlineData("DINNER", "Dinner")]
    [InlineData("nincs-ilyen", "Snack")]
    public void Meal_names_are_parsed_case_insensitively(string input, string expected)
    {
        CalorieService.ParseMeal(input).ToString().Should().Be(expected);
    }
}
