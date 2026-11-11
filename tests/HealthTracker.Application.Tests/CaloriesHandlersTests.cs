using AutoMapper;
using FluentAssertions;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Calories;
using HealthTracker.Application.Features.Calories.Commands.AddFoodEntry;
using HealthTracker.Application.Features.Calories.Commands.DeleteFoodEntry;
using HealthTracker.Application.Features.Calories.Commands.UpdateCalorieGoal;
using HealthTracker.Application.Features.Calories.Commands.UpdateFoodEntry;
using HealthTracker.Application.Features.Calories.Models;
using HealthTracker.Application.Features.Calories.Queries.GetCaloriesDay;
using HealthTracker.Application.Features.Calories.Queries.GetFoodEntry;
using HealthTracker.Domain.Entities;
using HealthTracker.Domain.Enums;
using Moq;
using Xunit;

namespace HealthTracker.Application.Tests;

public class CaloriesHandlersTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset Now = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 8, 16);

    private readonly Mock<IFoodEntryRepository> _repository = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly IMapper _mapper = TestMapper.Create();

    public CaloriesHandlersTests()
    {
        _clock.SetupGet(c => c.Now).Returns(Now);
        _currentUser.SetupGet(c => c.UserId).Returns(UserId);

        _currentUser.Setup(c => c.RequireUserId()).Returns(UserId);
        _repository
            .Setup(r => r.GetOrCreateGoalAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CalorieGoal.CreateDefault(UserId));
    }

    private static FoodEntry Entry(int kcal, MealType meal = MealType.Lunch, string name = "Étel") =>
        new(Guid.NewGuid(), UserId, Today, meal, name, kcal, Now);

    private static AddFoodEntryCommand AddCommand(
        string name = "Zabkása", int kcal = 320, string meal = "Breakfast") =>
        new(Today, meal, name, kcal);

    private Task<DayCaloriesResponse> GetDay(DateOnly? date = null) =>
        new GetCaloriesDayQueryHandler(_repository.Object, _clock.Object, _currentUser.Object, _mapper)
            .Handle(new GetCaloriesDayQuery(date), CancellationToken.None);

    private Task<FoodEntryItem> Add(AddFoodEntryCommand command) =>
        new AddFoodEntryCommandHandler(_repository.Object, _clock.Object, _currentUser.Object, _mapper)
            .Handle(command, CancellationToken.None);

    private Task<FoodEntryItem?> Update(UpdateFoodEntryCommand command) =>
        new UpdateFoodEntryCommandHandler(_repository.Object, _currentUser.Object, _mapper)
            .Handle(command, CancellationToken.None);

    private Task<bool> Delete(Guid id) =>
        new DeleteFoodEntryCommandHandler(_repository.Object, _currentUser.Object)
            .Handle(new DeleteFoodEntryCommand(id), CancellationToken.None);

    private Task<GoalResponse> UpdateGoal(int kcal) =>
        new UpdateCalorieGoalCommandHandler(_repository.Object, _currentUser.Object, _mapper)
            .Handle(new UpdateCalorieGoalCommand(kcal), CancellationToken.None);

    [Fact]
    public async Task GetDay_without_a_date_uses_today_from_the_clock()
    {
        _repository
            .Setup(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var day = await GetDay();

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

        var day = await GetDay(Today);

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

        var result = await Add(AddCommand());

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

        await Add(AddCommand(name: "  Alma  "));

        saved!.Name.Should().Be("Alma");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    [InlineData(20000)]
    public async Task Add_rejects_an_invalid_calorie_value(int kcal)
    {
        var call = async () => await Add(AddCommand(kcal: kcal));

        await call.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Add_rejects_an_empty_name()
    {
        var call = async () => await Add(AddCommand(name: "   "));

        await call.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Add_rejects_an_unknown_meal_and_saves_nothing()
    {
        var call = async () => await Add(AddCommand(meal: "brunch"));

        (await call.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().ContainSingle()
            .Which.Should().StartWith("Unknown meal: 'brunch'.");
        _repository.Verify(r => r.AddAsync(It.IsAny<FoodEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_rejects_invalid_input_before_looking_up_the_entry()
    {
        var call = async () => await Update(new UpdateFoodEntryCommand(Guid.NewGuid(), Today, "Lunch", "", 0));

        (await call.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().HaveCount(2);
        _repository.Verify(
            r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_returns_an_existing_entry()
    {
        var existing = Entry(320, MealType.Breakfast, "Zabkása");
        _repository
            .Setup(r => r.FindAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var handler = new GetFoodEntryQueryHandler(_repository.Object, _currentUser.Object, _mapper);

        var result = await handler.Handle(new GetFoodEntryQuery(existing.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(existing.Id);
        result.Name.Should().Be("Zabkása");
        result.Meal.Should().Be("Breakfast");
    }

    [Fact]
    public async Task Get_returns_null_for_someone_elses_entry()
    {
        var foreignId = Guid.NewGuid();

        _repository
            .Setup(r => r.FindAsync(UserId, foreignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FoodEntry?)null);
        var handler = new GetFoodEntryQueryHandler(_repository.Object, _currentUser.Object, _mapper);

        var result = await handler.Handle(new GetFoodEntryQuery(foreignId), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_changes_an_existing_entry()
    {
        var existing = Entry(320, MealType.Breakfast, "Zabkása");
        _repository
            .Setup(r => r.FindAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await Update(new UpdateFoodEntryCommand(existing.Id, Today, "Snack", "Müzli", 410));

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

        var result = await Update(new UpdateFoodEntryCommand(foreignId, Today, "Breakfast", "Zabkása", 320));

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

        var deleted = await Delete(existing.Id);

        deleted.Should().BeTrue();
        _repository.Verify(r => r.RemoveAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_returns_false_when_there_is_nothing_to_delete()
    {
        _repository
            .Setup(r => r.FindAsync(UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FoodEntry?)null);

        var deleted = await Delete(Guid.NewGuid());

        deleted.Should().BeFalse();
        _repository.Verify(r => r.RemoveAsync(It.IsAny<FoodEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task The_goal_can_be_changed()
    {
        var result = await UpdateGoal(2500);

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
        var call = async () => await UpdateGoal(kcal);

        await call.Should().ThrowAsync<ValidationException>();
        _repository.Verify(
            r => r.UpdateGoalAsync(It.IsAny<CalorieGoal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Every_query_is_scoped_to_the_logged_in_user()
    {
        _repository
            .Setup(r => r.GetForDateAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await GetDay(Today);

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
        MealTypeParser.Parse(input).ToString().Should().Be(expected);
    }
}
