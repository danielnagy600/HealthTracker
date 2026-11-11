using AutoMapper;
using FluentAssertions;
using HealthTracker.Application.Contracts;
using HealthTracker.Application.Contracts.Persistence;
using HealthTracker.Application.Exceptions;
using HealthTracker.Application.Features.Water.Commands.AddIntake;
using HealthTracker.Application.Features.Water.Commands.DeleteIntake;
using HealthTracker.Application.Features.Water.Models;
using HealthTracker.Application.Features.Water.Queries.GetIntake;
using HealthTracker.Application.Features.Water.Queries.GetWaterDay;
using HealthTracker.Application.Features.Water.Queries.GetWaterReminder;
using HealthTracker.Domain.Entities;
using HealthTracker.Domain.Enums;
using Moq;
using Xunit;

namespace HealthTracker.Application.Tests;

public class WaterHandlersTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset Now = new(2026, 7, 29, 15, 0, 0, TimeSpan.Zero);

    private readonly Mock<IWaterRepository> _repository = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly IMapper _mapper = TestMapper.Create();

    public WaterHandlersTests()
    {
        _clock.SetupGet(c => c.Now).Returns(Now);
        _currentUser.SetupGet(c => c.UserId).Returns(UserId);

        _currentUser.Setup(c => c.RequireUserId()).Returns(UserId);
        _repository
            .Setup(r => r.GetOrCreateSettingsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(WaterSettings.CreateDefault(UserId));
    }

    [Fact]
    public async Task AddIntake_saves_intake_for_current_user_and_today()
    {
        WaterIntake? saved = null;
        _repository
            .Setup(r => r.AddIntakeAsync(It.IsAny<WaterIntake>(), It.IsAny<CancellationToken>()))
            .Callback<WaterIntake, CancellationToken>((intake, _) => saved = intake)
            .Returns(Task.CompletedTask);
        var handler = new AddIntakeCommandHandler(_repository.Object, _clock.Object, _currentUser.Object, _mapper);

        var result = await handler.Handle(new AddIntakeCommand(500), CancellationToken.None);

        result.AmountMl.Should().Be(500);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(UserId);
        saved.Date.Should().Be(DateOnly.FromDateTime(Now.DateTime));
        _repository.Verify(
            r => r.AddIntakeAsync(It.IsAny<WaterIntake>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task AddIntake_rejects_a_non_positive_amount(int amountMl)
    {
        var handler = new AddIntakeCommandHandler(_repository.Object, _clock.Object, _currentUser.Object, _mapper);

        var call = async () => await handler.Handle(new AddIntakeCommand(amountMl), CancellationToken.None);

        (await call.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Equal("AmountMl must be positive.");
        _repository.Verify(
            r => r.AddIntakeAsync(It.IsAny<WaterIntake>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetIntake_returns_an_existing_intake()
    {
        var existing = new WaterIntake(
            Guid.NewGuid(), UserId, DateOnly.FromDateTime(Now.DateTime), Now, 300);
        _repository
            .Setup(r => r.FindIntakeAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var handler = new GetIntakeQueryHandler(_repository.Object, _currentUser.Object, _mapper);

        var result = await handler.Handle(new GetIntakeQuery(existing.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(existing.Id);
        result.AmountMl.Should().Be(300);
    }

    [Fact]
    public async Task GetIntake_returns_null_for_someone_elses_intake()
    {
        var foreignId = Guid.NewGuid();

        _repository
            .Setup(r => r.FindIntakeAsync(UserId, foreignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WaterIntake?)null);
        var handler = new GetIntakeQueryHandler(_repository.Object, _currentUser.Object, _mapper);

        var result = await handler.Handle(new GetIntakeQuery(foreignId), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDay_without_a_date_aggregates_todays_intakes()
    {
        var today = DateOnly.FromDateTime(Now.DateTime);
        _repository
            .Setup(r => r.GetIntakesForDateAsync(UserId, today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WaterIntake>
            {
                new(Guid.NewGuid(), UserId, today, Now.AddHours(-2), 500),
                new(Guid.NewGuid(), UserId, today, Now.AddHours(-1), 300),
            });

        var summary = await GetDay(new GetWaterDayQuery());

        summary.Date.Should().Be(today);
        summary.ConsumedMl.Should().Be(800);
        summary.TargetMl.Should().Be(2000);
        summary.RemainingMl.Should().Be(1200);
        summary.PercentComplete.Should().Be(40);
        summary.Intakes.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDay_with_a_date_reads_that_day()
    {
        var yesterday = DateOnly.FromDateTime(Now.DateTime).AddDays(-1);
        _repository
            .Setup(r => r.GetIntakesForDateAsync(UserId, yesterday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WaterIntake>
            {
                new(Guid.NewGuid(), UserId, yesterday, Now.AddDays(-1), 1500),
            });

        var summary = await GetDay(new GetWaterDayQuery(yesterday));

        summary.Date.Should().Be(yesterday);
        summary.ConsumedMl.Should().Be(1500);
        _repository.Verify(
            r => r.GetIntakesForDateAsync(UserId, yesterday, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteIntake_removes_an_existing_intake()
    {
        var existing = new WaterIntake(
            Guid.NewGuid(), UserId, DateOnly.FromDateTime(Now.DateTime), Now, 250);
        _repository
            .Setup(r => r.FindIntakeAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var handler = new DeleteIntakeCommandHandler(_repository.Object, _currentUser.Object);

        var deleted = await handler.Handle(new DeleteIntakeCommand(existing.Id), CancellationToken.None);

        deleted.Should().BeTrue();
        _repository.Verify(r => r.RemoveIntakeAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteIntake_returns_false_for_someone_elses_intake()
    {
        var foreignId = Guid.NewGuid();

        _repository
            .Setup(r => r.FindIntakeAsync(UserId, foreignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WaterIntake?)null);
        var handler = new DeleteIntakeCommandHandler(_repository.Object, _currentUser.Object);

        var deleted = await handler.Handle(new DeleteIntakeCommand(foreignId), CancellationToken.None);

        deleted.Should().BeFalse();
        _repository.Verify(
            r => r.RemoveIntakeAsync(It.IsAny<WaterIntake>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Reminder_reflects_consumed_amount()
    {
        var today = DateOnly.FromDateTime(Now.DateTime);
        _repository
            .Setup(r => r.GetIntakesForDateAsync(UserId, today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WaterIntake>
            {
                new(Guid.NewGuid(), UserId, today, Now.AddHours(-1), 200),
            });
        var handler = new GetWaterReminderQueryHandler(_repository.Object, _clock.Object, _currentUser.Object);

        var reminder = await handler.Handle(new GetWaterReminderQuery(), CancellationToken.None);

        reminder.ConsumedMl.Should().Be(200);
        reminder.Status.Should().Be(nameof(ReminderStatus.Behind));
        reminder.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Without_a_logged_in_user_the_handler_throws()
    {
        _currentUser.Setup(c => c.RequireUserId()).Throws<InvalidOperationException>();

        var act = async () => await GetDay(new GetWaterDayQuery());

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    private Task<DailySummaryResponse> GetDay(GetWaterDayQuery query) =>
        new GetWaterDayQueryHandler(_repository.Object, _clock.Object, _currentUser.Object, _mapper)
            .Handle(query, CancellationToken.None);
}
