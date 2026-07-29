namespace HealthTracker.Modules.Water.Application;

/// <summary>
/// A Water modul üzleti belépési pontja. A HTTP-végpontok ezt hívják.
/// A bejelentkezett felhasználót a szolgáltatás maga oldja fel (ICurrentUser),
/// ezért a metódusok nem kérnek userId paramétert.
/// </summary>
public interface IWaterService
{
    Task<IntakeItem> AddIntakeAsync(AddIntakeRequest request, CancellationToken ct = default);
    Task<DailySummaryResponse> GetTodaySummaryAsync(CancellationToken ct = default);
    Task<ReminderResponse> GetReminderAsync(CancellationToken ct = default);
    Task<SettingsResponse> GetSettingsAsync(CancellationToken ct = default);
    Task<SettingsResponse> UpdateSettingsAsync(UpdateSettingsRequest request, CancellationToken ct = default);
}
