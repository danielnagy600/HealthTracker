using HealthTracker.Modules.Schedule.Domain;
using HealthTracker.SharedKernel.Abstractions;

namespace HealthTracker.Modules.Schedule.Application;

/// <summary>
/// A Schedule modul üzleti logikája. Összeköti a tárolót (IActivityRepository),
/// az órát (IClock) és a bejelentkezett felhasználót (ICurrentUser) a tiszta
/// domain-számítással (DayPlanCalculator).
/// </summary>
public sealed class ScheduleService : IScheduleService
{
    private readonly IActivityRepository _repository;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public ScheduleService(IActivityRepository repository, IClock clock, ICurrentUser currentUser)
    {
        _repository = repository;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<DayScheduleResponse> GetDayAsync(DateOnly? date = null, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var day = date ?? DateOnly.FromDateTime(_clock.Now.DateTime);

        var activities = await _repository.GetForDateAsync(userId, day, ct);

        // A tiszta domain-számítás – ezt a tesztek külön is ellenőrzik.
        var (windowStart, windowEnd) = DayPlanCalculator.WindowFor(activities);
        var plan = DayPlanCalculator.Calculate(activities, windowStart, windowEnd);

        var items = activities
            .OrderBy(a => a.StartTime)
            .ThenBy(a => a.EndTime)
            .Select(ToItem)
            .ToList();

        var freeSlots = plan.FreeSlots
            .Select(s => new TimeSlot(s.Start, s.End, s.DurationMinutes))
            .ToList();

        var conflicts = plan.Conflicts
            .Select(c => new ConflictItem(
                c.First.Id, c.Second.Id, c.First.Title, c.Second.Title,
                c.Overlap.Start, c.Overlap.End, c.Overlap.DurationMinutes))
            .ToList();

        return new DayScheduleResponse(
            day, plan.WindowStart, plan.WindowEnd, plan.BusyMinutes, plan.FreeMinutes,
            items, freeSlots, conflicts);
    }

    public async Task<ActivityItem> AddAsync(SaveActivityRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var activity = new Activity(
            Guid.NewGuid(), userId, request.Date, request.StartTime, request.EndTime,
            request.Title, ParseColor(request.Color), request.Note);

        await _repository.AddAsync(activity, ct);
        return ToItem(activity);
    }

    public async Task<ActivityItem?> UpdateAsync(Guid id, SaveActivityRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var activity = await _repository.FindAsync(userId, id, ct);
        if (activity is null)
            return null;

        activity.Update(
            request.Date, request.StartTime, request.EndTime,
            request.Title, ParseColor(request.Color), request.Note);

        await _repository.UpdateAsync(activity, ct);
        return ToItem(activity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var activity = await _repository.FindAsync(userId, id, ct);
        if (activity is null)
            return false;

        await _repository.RemoveAsync(activity, ct);
        return true;
    }

    /// <summary>A színnév feloldása; ismeretlen név esetén az alapértelmezett kék.</summary>
    public static ActivityColor ParseColor(string? color) =>
        Enum.TryParse<ActivityColor>(color, ignoreCase: true, out var parsed)
            ? parsed
            : ActivityColor.Blue;

    /// <summary>Igaz, ha a megadott szín szerepel a palettán.</summary>
    public static bool IsKnownColor(string? color) =>
        Enum.TryParse<ActivityColor>(color, ignoreCase: true, out _);

    private static ActivityItem ToItem(Activity a) =>
        new(a.Id, a.Date, a.StartTime, a.EndTime, a.Title, a.Color.ToString(), a.Note, a.DurationMinutes);
}
