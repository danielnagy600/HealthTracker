using FluentAssertions;
using HealthTracker.Modules.Water.Application;
using HealthTracker.Modules.Water.Domain;
using HealthTracker.SharedKernel.Abstractions;
using Moq;
using Xunit;

namespace HealthTracker.Modules.Water.Tests;

/// <summary>
/// A WaterService tesztjei Moq-kal: a függőségeket (tároló, óra, felhasználó)
/// mockoljuk, így a szolgáltatás logikáját elszigetelten vizsgáljuk.
/// </summary>
public class WaterServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTimeOffset Now = new(2026, 7, 29, 15, 0, 0, TimeSpan.Zero);

    private readonly Mock<IWaterRepository> _repository = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly WaterService _sut;

    public WaterServiceTests()
    {
        _clock.SetupGet(c => c.Now).Returns(Now);
        _currentUser.SetupGet(c => c.UserId).Returns(UserId);
        // A RequireUserId() default interface metódus törzsét a Moq nem futtatja,
        // ezért explicit beállítjuk, mit adjon vissza.
        _currentUser.Setup(c => c.RequireUserId()).Returns(UserId);
        _repository
            .Setup(r => r.GetOrCreateSettingsAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(WaterSettings.CreateDefault(UserId));

        _sut = new WaterService(_repository.Object, _clock.Object, _currentUser.Object);
    }

    [Fact]
    public async Task AddIntake_saves_intake_for_current_user_and_today()
    {
        WaterIntake? saved = null;
        _repository
            .Setup(r => r.AddIntakeAsync(It.IsAny<WaterIntake>(), It.IsAny<CancellationToken>()))
            .Callback<WaterIntake, CancellationToken>((intake, _) => saved = intake)
            .Returns(Task.CompletedTask);

        var result = await _sut.AddIntakeAsync(new AddIntakeRequest(500));

        result.AmountMl.Should().Be(500);
        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(UserId);
        saved.Date.Should().Be(DateOnly.FromDateTime(Now.DateTime));
        _repository.Verify(
            r => r.AddIntakeAsync(It.IsAny<WaterIntake>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task TodaySummary_aggregates_todays_intakes()
    {
        var today = DateOnly.FromDateTime(Now.DateTime);
        _repository
            .Setup(r => r.GetIntakesForDateAsync(UserId, today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WaterIntake>
            {
                new(Guid.NewGuid(), UserId, today, Now.AddHours(-2), 500),
                new(Guid.NewGuid(), UserId, today, Now.AddHours(-1), 300),
            });

        var summary = await _sut.GetTodaySummaryAsync();

        summary.ConsumedMl.Should().Be(800);
        summary.TargetMl.Should().Be(2000);
        summary.RemainingMl.Should().Be(1200);
        summary.PercentComplete.Should().Be(40);
        summary.Intakes.Should().HaveCount(2);
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

        var reminder = await _sut.GetReminderAsync();

        reminder.ConsumedMl.Should().Be(200);
        reminder.Status.Should().Be(nameof(ReminderStatus.Behind));
        reminder.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Without_a_logged_in_user_the_service_throws()
    {
        _currentUser.Setup(c => c.RequireUserId()).Throws<InvalidOperationException>();

        var act = async () => await _sut.GetTodaySummaryAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
