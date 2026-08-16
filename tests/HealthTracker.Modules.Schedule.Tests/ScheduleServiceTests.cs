using FluentAssertions;
using HealthTracker.Modules.Schedule.Application;
using HealthTracker.Modules.Schedule.Domain;
using HealthTracker.SharedKernel.Abstractions;
using Moq;
using Xunit;

namespace HealthTracker.Modules.Schedule.Tests;

/// <summary>
/// A ScheduleService tesztjei Moq-kal: a függőségeket (tároló, óra, felhasználó)
/// mockoljuk, így a szolgáltatás logikáját elszigetelten vizsgáljuk.
/// </summary>
public class ScheduleServiceTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly DateTimeOffset Now = new(2026, 8, 16, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Today = new(2026, 8, 16);

    private readonly Mock<IActivityRepository> _repository = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly ScheduleService _sut;

    public ScheduleServiceTests()
    {
        _clock.SetupGet(c => c.Now).Returns(Now);
        _currentUser.SetupGet(c => c.UserId).Returns(UserId);
        // A RequireUserId() default interface metódus törzsét a Moq nem futtatja,
        // ezért explicit beállítjuk, mit adjon vissza.
        _currentUser.Setup(c => c.RequireUserId()).Returns(UserId);

        _sut = new ScheduleService(_repository.Object, _clock.Object, _currentUser.Object);
    }

    private static Activity Act(int fromHour, int toHour, string title = "Teendő") =>
        new(Guid.NewGuid(), UserId, Today,
            new TimeOnly(fromHour, 0), new TimeOnly(toHour, 0), title, ActivityColor.Blue);

    private static SaveActivityRequest Request(
        int fromHour = 9, int toHour = 10, string title = "Megbeszélés",
        string color = "Green", string? note = "Q3 tervek") =>
        new(Today, new TimeOnly(fromHour, 0), new TimeOnly(toHour, 0), title, color, note);

    [Fact]
    public async Task GetDay_without_a_date_uses_today_from_the_clock()
    {
        _repository
            .Setup(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var day = await _sut.GetDayAsync();

        day.Date.Should().Be(Today);
        _repository.Verify(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDay_returns_activities_sorted_by_start_time()
    {
        _repository
            .Setup(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Act(13, 14, "Ebéd"), Act(8, 9, "Edzés")]);

        var day = await _sut.GetDayAsync(Today);

        day.Activities.Select(a => a.Title).Should().ContainInOrder("Edzés", "Ebéd");
    }

    [Fact]
    public async Task GetDay_reports_busy_time_and_conflicts_from_the_domain()
    {
        _repository
            .Setup(r => r.GetForDateAsync(UserId, Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync([Act(9, 11, "Megbeszélés"), Act(10, 12, "Edzés")]);

        var day = await _sut.GetDayAsync(Today);

        day.BusyMinutes.Should().Be(180); // az átfedés nem számít duplán
        day.Conflicts.Should().ContainSingle();
        day.Conflicts[0].OverlapMinutes.Should().Be(60);
        day.Conflicts[0].FirstTitle.Should().Be("Megbeszélés");
    }

    [Fact]
    public async Task Add_saves_the_activity_for_the_current_user()
    {
        Activity? saved = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
            .Callback<Activity, CancellationToken>((a, _) => saved = a)
            .Returns(Task.CompletedTask);

        var result = await _sut.AddAsync(Request());

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(UserId);
        saved.Title.Should().Be("Megbeszélés");
        saved.Color.Should().Be(ActivityColor.Green);
        saved.Note.Should().Be("Q3 tervek");
        result.DurationMinutes.Should().Be(60);
        result.Color.Should().Be("Green");
    }

    [Fact]
    public async Task Add_rejects_an_end_time_before_the_start()
    {
        var call = async () => await _sut.AddAsync(Request(fromHour: 11, toHour: 10));

        await call.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Add_trims_the_title_and_drops_an_empty_note()
    {
        Activity? saved = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()))
            .Callback<Activity, CancellationToken>((a, _) => saved = a)
            .Returns(Task.CompletedTask);

        await _sut.AddAsync(Request(title: "  Edzés  ", note: "   "));

        saved!.Title.Should().Be("Edzés");
        saved.Note.Should().BeNull();
    }

    [Fact]
    public async Task Update_changes_an_existing_activity()
    {
        var existing = Act(9, 10, "Régi cím");
        _repository
            .Setup(r => r.FindAsync(UserId, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(existing.Id, Request(fromHour: 14, toHour: 16, title: "Új cím", color: "Red"));

        result.Should().NotBeNull();
        result!.Title.Should().Be("Új cím");
        result.Color.Should().Be("Red");
        result.DurationMinutes.Should().Be(120);
        _repository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_returns_null_for_someone_elses_activity()
    {
        var foreignId = Guid.NewGuid();
        // A tároló a felhasználóra szűr, ezért nem talál semmit.
        _repository
            .Setup(r => r.FindAsync(UserId, foreignId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await _sut.UpdateAsync(foreignId, Request());

        result.Should().BeNull();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_removes_an_existing_activity()
    {
        var existing = Act(9, 10);
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
            .ReturnsAsync((Activity?)null);

        var deleted = await _sut.DeleteAsync(Guid.NewGuid());

        deleted.Should().BeFalse();
        _repository.Verify(r => r.RemoveAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Every_query_is_scoped_to_the_logged_in_user()
    {
        _repository
            .Setup(r => r.GetForDateAsync(It.IsAny<Guid>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.GetDayAsync(Today);

        // Soha nem kérdezünk le más felhasználó adatait.
        _repository.Verify(
            r => r.GetForDateAsync(OtherUserId, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("Blue", "Blue")]
    [InlineData("green", "Green")]
    [InlineData("PURPLE", "Purple")]
    [InlineData("nincs-ilyen", "Blue")] // ismeretlen szín → alapértelmezett
    public void Color_names_are_parsed_case_insensitively(string input, string expected)
    {
        ScheduleService.ParseColor(input).ToString().Should().Be(expected);
    }
}
