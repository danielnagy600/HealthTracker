using HealthTracker.Modules.Water.Domain;
using HealthTracker.SharedKernel.Abstractions;

namespace HealthTracker.Modules.Water.Application;

/// <summary>
/// A Water modul üzleti logikája. Összeköti a tárolót (IWaterRepository),
/// az órát (IClock) és a bejelentkezett felhasználót (ICurrentUser) a tiszta
/// domain-számítással (WaterReminderCalculator).
/// </summary>
public sealed class WaterService : IWaterService
{
    private readonly IWaterRepository _repository;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public WaterService(IWaterRepository repository, IClock clock, ICurrentUser currentUser)
    {
        _repository = repository;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IntakeItem> AddIntakeAsync(AddIntakeRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var now = _clock.Now;
        var today = DateOnly.FromDateTime(now.DateTime);

        // A "nap" a helyi dátum (a felhasználó napja), az időbélyeget viszont UTC-ben
        // tároljuk – a PostgreSQL "timestamp with time zone" csak UTC offszetet fogad el.
        var intake = new WaterIntake(Guid.NewGuid(), userId, today, now.ToUniversalTime(), request.AmountMl);
        await _repository.AddIntakeAsync(intake, ct);

        return new IntakeItem(intake.Id, intake.RecordedAt, intake.AmountMl);
    }

    public async Task<DailySummaryResponse> GetTodaySummaryAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var today = DateOnly.FromDateTime(_clock.Now.DateTime);

        var settings = await _repository.GetOrCreateSettingsAsync(userId, ct);
        var intakes = await _repository.GetIntakesForDateAsync(userId, today, ct);

        var consumed = intakes.Sum(i => i.AmountMl);
        var remaining = Math.Max(0, settings.DailyTargetMl - consumed);
        var percent = settings.DailyTargetMl == 0
            ? 0
            : Math.Round(100.0 * consumed / settings.DailyTargetMl, 1);

        var items = intakes
            .OrderBy(i => i.RecordedAt)
            .Select(i => new IntakeItem(i.Id, i.RecordedAt, i.AmountMl))
            .ToList();

        return new DailySummaryResponse(today, settings.DailyTargetMl, consumed, remaining, percent, items);
    }

    public async Task<ReminderResponse> GetReminderAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var now = _clock.Now;
        var today = DateOnly.FromDateTime(now.DateTime);

        var settings = await _repository.GetOrCreateSettingsAsync(userId, ct);
        var intakes = await _repository.GetIntakesForDateAsync(userId, today, ct);
        var consumed = intakes.Sum(i => i.AmountMl);

        // A tiszta domain-számítás – ez az, amit a tesztek külön is ellenőriznek.
        var r = WaterReminderCalculator.Calculate(settings, consumed, now);

        return new ReminderResponse(
            r.ConsumedMl, r.TargetMl, r.RemainingMl, r.ExpectedByNowMl, r.DeficitMl,
            r.Status.ToString(), r.NextDoseMl, r.NextReminderAt, r.Message);
    }

    public async Task<SettingsResponse> GetSettingsAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var s = await _repository.GetOrCreateSettingsAsync(userId, ct);
        return new SettingsResponse(s.DailyTargetMl, s.WakeTime, s.SleepTime);
    }

    public async Task<SettingsResponse> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var s = await _repository.GetOrCreateSettingsAsync(userId, ct);

        s.DailyTargetMl = request.DailyTargetMl;
        s.WakeTime = request.WakeTime;
        s.SleepTime = request.SleepTime;

        await _repository.UpdateSettingsAsync(s, ct);
        return new SettingsResponse(s.DailyTargetMl, s.WakeTime, s.SleepTime);
    }
}
